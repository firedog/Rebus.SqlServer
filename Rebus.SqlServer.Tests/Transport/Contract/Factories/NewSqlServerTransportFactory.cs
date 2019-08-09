﻿using System;
using System.Collections.Generic;
using Rebus.Extensions;
using Rebus.Logging;
using Rebus.SqlServer.Transport;
using Rebus.Tests.Contracts.Transports;
using Rebus.Threading.TaskParallelLibrary;
using Rebus.Time;
using Rebus.Transport;
// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleStringLiteral

namespace Rebus.SqlServer.Tests.Transport.Contract.Factories
{
    public class NewSqlServerTransportFactory : ITransportFactory
    {
        readonly HashSet<string> _tablesToDrop = new HashSet<string>();
        readonly List<IDisposable> _disposables = new List<IDisposable>();

        public NewSqlServerTransportFactory() => SqlTestHelper.DropAllTables();

        public ITransport CreateOneWayClient()
        {
            var rebusTime = new DefaultRebusTime();
            var consoleLoggerFactory = new ConsoleLoggerFactory(false);
            var connectionProvider = new DbConnectionProvider(SqlTestHelper.ConnectionString, consoleLoggerFactory);
            var asyncTaskFactory = new TplAsyncTaskFactory(consoleLoggerFactory);
            var timingConfiguration = new TimingConfiguration();
            var transport = new NewSqlServerTransport(
                connectionProvider: connectionProvider,
                rebusTime: rebusTime,
                asyncTaskFactory: asyncTaskFactory,
                rebusLoggerFactory: consoleLoggerFactory,
                inputQueueName: null,
                schema: "dbo",
                timingConfiguration: timingConfiguration
            );

            _disposables.Add(transport);

            transport.Initialize();

            return transport;
        }

        public ITransport Create(string inputQueueAddress)
        {
            var rebusTime = new DefaultRebusTime();
            var consoleLoggerFactory = new ConsoleLoggerFactory(false);
            var connectionProvider = new DbConnectionProvider(SqlTestHelper.ConnectionString, consoleLoggerFactory);
            var asyncTaskFactory = new TplAsyncTaskFactory(consoleLoggerFactory);
            var timingConfiguration = new TimingConfiguration();
            var transport = new NewSqlServerTransport(
                connectionProvider: connectionProvider,
                rebusTime: rebusTime,
                asyncTaskFactory: asyncTaskFactory,
                rebusLoggerFactory: consoleLoggerFactory,
                inputQueueName: inputQueueAddress,
                schema: "dbo",
                timingConfiguration: timingConfiguration
            );

            _disposables.Add(transport);

            transport.Initialize();

            _tablesToDrop.Add($"[dbo].[{inputQueueAddress}]");

            return transport;
        }

        public void CleanUp()
        {
            _disposables.ForEach(d => d.Dispose());
            _disposables.Clear();

            _tablesToDrop.ForEach(SqlTestHelper.DropTable);
            _tablesToDrop.Clear();
        }
    }
}