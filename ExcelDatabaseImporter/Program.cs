using System.Data;
using ExcelDataReader;
using Microsoft.Data.SqlClient;

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
		
		DataTable table = result.Tables[0];
		int i = 0;
		foreach (DataRow row in table.Rows)
        {
			string existsQ = "SELECT CardCode from dbo.Clients WHERE CardCode = @CardCode";
			using (SqlCommand existsC = new SqlCommand(existsQ, connection))
			{
                existsC.Parameters.Add("@CardCode", SqlDbType.VarChar, 255).Value = row["CardCode"].ToString() ?? string.Empty;
				object? found = existsC.ExecuteScalar();

				if(found == null)
				{
					string insertQ = "INSERT INTO dbo.Clients (CardCode, LastName, FirstName, SurName, PhoneMobile, Email, GenderId, Birthday, City, Pincode, Bonus, Turnover) VALUES (@CardCode, @LastName, @FirstName, @SurName, @PhoneMobile, @Email, @GenderId, @Birthday, @City, @Pincode, @Bonus, @Turnover)";
					using (SqlCommand insertC = new SqlCommand(insertQ, connection))
					{
						if (!DateTime.TryParse(row["Birthday"].ToString(), out DateTime birthday))
						{
							continue;
						}

						insertC.Parameters.Add("@CardCode", SqlDbType.VarChar, 255).Value = row["CardCode"].ToString();
                        insertC.Parameters.Add("@LastName", SqlDbType.VarChar, 255).Value = row["LastName"].ToString();
                        insertC.Parameters.Add("@FirstName", SqlDbType.VarChar, 255).Value = row["FirstName"].ToString();
                        insertC.Parameters.Add("@SurName", SqlDbType.VarChar, 255).Value = row["SurName"]?.ToString() ?? string.Empty;
                        insertC.Parameters.Add("@PhoneMobile", SqlDbType.VarChar, 255).Value = row["PhoneMobile"].ToString();
                        insertC.Parameters.Add("@Email", SqlDbType.VarChar, 300).Value = row["Email"]?.ToString() ?? string.Empty;
                        insertC.Parameters.Add("@GenderId", SqlDbType.VarChar, 10).Value = row["GenderId"].ToString();
                        insertC.Parameters.Add("@Birthday", SqlDbType.Date).Value = birthday;
                        insertC.Parameters.Add("@City", SqlDbType.VarChar, 255).Value = row["City"]?.ToString() ?? string.Empty;
                        insertC.Parameters.Add("@Pincode", SqlDbType.Int).Value = row["Pincode"] ?? null;
                        insertC.Parameters.Add("@Bonus", SqlDbType.Int).Value = row["Bonus"] ?? null;
						insertC.Parameters.Add("@Turnover", SqlDbType.Int).Value = row["Turnover"] ?? null;

						insertC.ExecuteNonQuery();
						Console.WriteLine("Вставлен CardCode #" + row["CardCode"].ToString());
                    }
				}
            }

        }
		Console.WriteLine("Готово! Нажмите любую кнопку для выхода.");
		Console.ReadLine();
	}
}