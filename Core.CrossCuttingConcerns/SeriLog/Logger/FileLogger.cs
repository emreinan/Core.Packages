using Core.CrossCuttingConcerns.SeriLog.ConfigurationModels;
using Core.CrossCuttingConcerns.SeriLog.Messages;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CrossCuttingConcerns.SeriLog.Logger;

public class FileLogger : LoggerServiceBase
{
	public FileLogger(IConfiguration configuration)
	{
		var logConfiguration = configuration.GetSection("SeriLogConfigurations:FileLogConfiguration").Get<FileLogConfiguration>()
			?? throw new Exception(SerilogMessages.NullOptionsMessage);

		var logFilePath = string.Format(format: "{0}{1}.txt", arg0: Directory.GetCurrentDirectory() + logConfiguration.FolderPath, arg1: ".txt");

		Logger = new LoggerConfiguration().WriteTo.File(
			logFilePath, 
			rollingInterval: RollingInterval.Day, // Log file will be created daily
			retainedFileCountLimit: null, // Log files will be kept indefinitely
			fileSizeLimitBytes: 50000000, // Log file size limit is 50 MB
			outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}" // Log file format
			).CreateLogger();
	}
}
