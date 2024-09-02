using Core.CrossCuttingConcerns.SeriLog.ConfigurationModels;
using Core.CrossCuttingConcerns.SeriLog.Messages;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CrossCuttingConcerns.SeriLog.Logger;

public class MsSqlLogger : LoggerServiceBase
{
    public MsSqlLogger(IConfiguration configuration)
    {
        var logConfiguration = configuration.GetSection("SeriLogConfigurations:MsSqlLogConfiguration").Get<MsSqlConfiguration>()
            ?? throw new Exception(SerilogMessages.NullOptionsMessage);

		var sinkOpts = new MSSqlServerSinkOptions
		{
			TableName = logConfiguration.TableName,
			AutoCreateSqlTable = logConfiguration.AutoCreateSqlTable,
		};

		var columnOptions = new ColumnOptions();

		var serilogLogger = new LoggerConfiguration()
			.WriteTo.MSSqlServer(
				connectionString: logConfiguration.ConnectionString,
				sinkOptions: sinkOpts,
				columnOptions: columnOptions
			).CreateLogger();

		Logger = serilogLogger;
	}
}
