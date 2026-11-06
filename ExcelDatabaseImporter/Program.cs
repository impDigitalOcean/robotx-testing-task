using ExcelDataReader;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

// сначала подключение к БД
Console.WriteLine("Введите строку подключения к базе данных: ");
string? connectionString = Console.ReadLine();

if(connectionString == null)
{
	Console.WriteLine("Строка подключения не может быть пустой. Пожалуйста, попробуйте снова.");
	return;
}

using var connection = new SqlConnection(connectionString);
try
{
    connection.Open();
    Console.WriteLine("Подключение к базе данных успешно установлено.");
}
catch (Exception ex)
{
    Console.WriteLine($"Ошибка подключения к базе данных: {ex.Message}");
    return;
}
// далее обработка Excel файла
Console.WriteLine("Введите путь к файлу Excel: ");
string? filePath = Console.ReadLine();

if(filePath == null || !File.Exists(filePath))
{
	Console.WriteLine("Файл не найден. Пожалуйста, проверьте путь и попробуйте снова.");
	return;
}

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
{
	using (var reader = ExcelReaderFactory.CreateReader(stream)) 
	{
		// первая строка всегда содержит названия столбцов
		var result = reader.AsDataSet(new ExcelDataSetConfiguration()
		{
			ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
			{
				UseHeaderRow = true
			}
		});

        var existing = new HashSet<int>();
        using (var existC = new SqlCommand("SELECT CardCode FROM dbo.Clients", connection))
        using (var existR = existC.ExecuteReader())
            while (existR.Read()) existing.Add(existR.GetInt32(0));

        DataTable table = result.Tables[0];
        using var trans = connection.BeginTransaction();
		string insertQ = """
			INSERT INTO dbo.Clients (CardCode, LastName, FirstName, SurName, PhoneMobile, Email, GenderId, Birthday, City, Pincode, Bonus, Turnover) 
			VALUES (@CardCode, @LastName, @FirstName, @SurName, @PhoneMobile, @Email, @GenderId, @Birthday, @City, @Pincode, @Bonus, @Turnover)
			""";

        string updateQ = """
                    UPDATE dbo.Clients 
                    SET LastName = @LastName, FirstName = @FirstName, SurName = @SurName, PhoneMobile = @PhoneMobile, Email = @Email, GenderId = @GenderId, Birthday = @Birthday, City = @City, Pincode = @Pincode, Bonus = @Bonus, Turnover = @Turnover
                    WHERE CardCode = @CardCode
                    """;

        int inserted = 0, updated = 0, skipped = 0, failed = 0;
        foreach (DataRow row in table.Rows)
        {
            if (!int.TryParse(row["CardCode"]?.ToString()?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int cardCode))
            {
                skipped++;

                Console.WriteLine("Некорректный cardcode, пропускаю...");
                continue;
            }

            if (!DateTime.TryParse(row["Birthday"].ToString(), CultureInfo.GetCultureInfo("ru-RU"), out DateTime birthday))
			{
                skipped++;

                Console.WriteLine("Некорректный Birthday, пропускаю...");
                continue;
			}

            if(existing.Contains(cardCode))
            {
                try
                {
                    using var updateC = new SqlCommand(updateQ, connection, trans);
                    updateC.Parameters.Add("@CardCode", SqlDbType.Int).Value = cardCode;
                    updateC.Parameters.Add("@LastName", SqlDbType.NVarChar, 255).Value = row["LastName"]?.ToString() ?? "";
                    updateC.Parameters.Add("@FirstName", SqlDbType.NVarChar, 255).Value = row["FirstName"]?.ToString() ?? "";
                    updateC.Parameters.Add("@SurName", SqlDbType.NVarChar, 255).Value = row["SurName"]?.ToString() ?? "";
                    updateC.Parameters.Add("@PhoneMobile", SqlDbType.NVarChar, 255).Value = row["PhoneMobile"]?.ToString() ?? "";
                    updateC.Parameters.Add("@Email", SqlDbType.NVarChar, 300).Value = row["Email"]?.ToString() ?? "";
                    updateC.Parameters.Add("@GenderId", SqlDbType.NVarChar, 10).Value = row["GenderId"]?.ToString() ?? "";
                    updateC.Parameters.Add("@Birthday", SqlDbType.Date).Value = birthday;
                    updateC.Parameters.Add("@City", SqlDbType.NVarChar, 255).Value = row["City"]?.ToString() ?? "";
                    updateC.Parameters.Add("@Pincode", SqlDbType.Int).Value = row["Pincode"] is DBNull ? DBNull.Value : Convert.ToInt32(row["Pincode"]);
                    updateC.Parameters.Add("@Bonus", SqlDbType.Int).Value = row["Bonus"] is DBNull ? DBNull.Value : Convert.ToInt32(row["Bonus"]);
                    updateC.Parameters.Add("@Turnover", SqlDbType.Int).Value = row["Turnover"] is DBNull ? DBNull.Value : Convert.ToInt32(row["Turnover"]);

                    updateC.ExecuteNonQuery();
                    updated++;
                }
                catch (Exception err)
                {
                    failed++;
                    Console.WriteLine("Ошибка: " + err.Message);
                }

                continue;
            }

			try
			{
				using var insertC = new SqlCommand(insertQ, connection, trans);
                insertC.Parameters.Add("@CardCode", SqlDbType.Int).Value = cardCode;
                insertC.Parameters.Add("@LastName", SqlDbType.NVarChar, 255).Value = row["LastName"]?.ToString() ?? "";
                insertC.Parameters.Add("@FirstName", SqlDbType.NVarChar, 255).Value = row["FirstName"]?.ToString() ?? "";
                insertC.Parameters.Add("@SurName", SqlDbType.NVarChar, 255).Value = row["SurName"]?.ToString() ?? "";
                insertC.Parameters.Add("@PhoneMobile", SqlDbType.NVarChar, 255).Value = row["PhoneMobile"]?.ToString() ?? "";
                insertC.Parameters.Add("@Email", SqlDbType.NVarChar, 300).Value = row["Email"]?.ToString() ?? "";
                insertC.Parameters.Add("@GenderId", SqlDbType.NVarChar, 10).Value = row["GenderId"]?.ToString() ?? "";
                insertC.Parameters.Add("@Birthday", SqlDbType.Date).Value = birthday;
                insertC.Parameters.Add("@City", SqlDbType.NVarChar, 255).Value = row["City"]?.ToString() ?? "";
                insertC.Parameters.Add("@Pincode", SqlDbType.Int).Value = row["Pincode"] is DBNull ? DBNull.Value : Convert.ToInt32(row["Pincode"]);
                insertC.Parameters.Add("@Bonus", SqlDbType.Int).Value = row["Bonus"] is DBNull ? DBNull.Value : Convert.ToInt32(row["Bonus"]);
                insertC.Parameters.Add("@Turnover", SqlDbType.Int).Value = row["Turnover"] is DBNull ? DBNull.Value : Convert.ToInt32(row["Turnover"]);

                insertC.ExecuteNonQuery();
                existing.Add(cardCode);

                inserted++;
            } catch(Exception err)
            {
                failed++;
                Console.WriteLine("Ошибка: " + err.Message);
            }
        }

        trans.Commit();
		Console.WriteLine($"Готово! Добавлено: {inserted}, обновлено: {updated}, ошибок: {failed}, пропущено: {skipped}. Нажмите любую кнопку для выхода.");
		Console.ReadLine();
	}
}