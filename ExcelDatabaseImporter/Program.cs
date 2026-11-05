using System.Data;
using ExcelDataReader;

// обработка Excel файла
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
			if(i == 10) {
				break;
			}
			i++;
			
            Console.WriteLine($"CardCode: {row["CardCode"]}, Name: {row["FirstName"]}");
        }
	}
}