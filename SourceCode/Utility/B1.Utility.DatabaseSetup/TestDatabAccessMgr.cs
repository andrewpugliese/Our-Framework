using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data.Common;
using System.Data.Objects.SqlClient;
using System.Diagnostics;
using System.Data;

using Microsoft.Practices.EnterpriseLibrary.Data;

using B1.Core;
using B1.DataAccess;

namespace B1.Utility.DatabaseSetup
{
    /// <summary>
    /// This is a helper class to help with unit testing.  It will demonstrate concurrent multi threaded database operations using dynamic
    /// SQL that is database independent. 
    /// <para>
    /// After you have a successfully built or altered the database, you can begin testing.
    /// <para>
    /// Note: First time may seem slow if running locally with a database server as the server
    /// may need to startup.  (most pronounced with local oracle)	
    /// </para>
    /// </para>
    /// <list type="bullets">
    /// <item>
    /// <para>
    /// RowCount button <see cref="B1.Utility.DatabaseSetup.DbSetupMgr.btnRowcount_Click"/>
    /// </para>
    /// </item>
    /// <item>
    /// <para>
    /// StartInsert button <see cref="B1.Utility.DatabaseSetup.TestDataAccessMgr.StartInsert"/>
    /// </para>
    /// </item>
    /// <item>
    /// <para>
    /// StartMerge button <see cref="B1.Utility.DatabaseSetup.TestDataAccessMgr.StartMerge"/>
    /// </para>
    /// </item>
    /// <item>
    /// <para>
    /// StartUpdate button <see cref="B1.Utility.DatabaseSetup.TestDataAccessMgr.StartUpdate"/>
    /// </para>
    /// </item>
    /// <item>
    /// <para>
    /// StartDelete button <see cref="B1.Utility.DatabaseSetup.TestDataAccessMgr.StartDelete"/>
    /// </para>
    /// </item>
    /// <item>
    /// <para>
    /// TraceLevel <see cref="B1.Utility.DatabaseSetup.DbSetupMgr.StartInsert"/>
    /// </para>
    /// </item>
    /// <item>
    /// <para>
    /// TransactionTest <see cref="B1.Utility.DatabaseSetup.DbSetupMgr.cmbTraceLevel_SelectedIndexChanged"/>
    /// </para>
    /// </item>
    /// </list>
    /// </summary>
    internal class TestDataAccessMgr
    {
        internal delegate void TestStoppedHandler(DbSetupMgr.TestTypeEnum testType);
        int _threadCount = 0;
        DataAccessMgr _daMgr = null;
        bool _stop = false;
        bool _stopInsert = false;
        bool _stopUpdate = false;
        bool _stopDelete = false;
        bool _stopMerge = false;
        static string _largeObject = "Z".PadLeft(Int16.MaxValue, 'Z');
        TestStoppedHandler _testStoppedHdlr = null;

        internal TestDataAccessMgr(DataAccessMgr daMgr
                , int threadCount
                , TestStoppedHandler testStoppedHdlr)
        {
            _threadCount = threadCount;
            _daMgr = daMgr;
            _testStoppedHdlr = testStoppedHdlr;
        }

        internal DataAccessMgr DaMgr
        {
            get { return _daMgr; }
        }

        /// <summary>
        /// When StartInsert button is clicked, function will launch a four new threads and they will
        /// begin insert the rows in the TestSequence table.
        /// <para>
        /// This will continue until the StopInsert button is clicked.
        /// </para>
        /// <para>
        /// All the threads will be inserting at the same time.
        /// </para>
        /// </summary>
        internal void StartInsert()
        {
            _stop = _stopInsert = false;

            for (int i = 0; i < _threadCount; i++)
            {
                Thread t = new Thread(Insert);
                t.Start();
            }

        }

        /// <summary>
        /// When StartUpdate button is clicked, function will launch a two new threads and they will
        /// begin updating the rows in the TestSequence table starting from the first until the last.
        /// <para>
        /// This will continue until either the StopUpdate button is clicked or all the rows are updated.
        /// </para>
        /// <para>
        /// One thread will update rows with even AppSequenceId; the other will update rows with odd id's
        /// </para>
        /// </summary>
        internal void StartUpdate()
        {
            _stop = _stopUpdate = false;

            Thread tEven = new Thread(Update);
            tEven.Start(true);
            Thread tOdd = new Thread(Update);
            tOdd.Start(false);

        }

        /// <summary>
        /// When StartMerge button is clicked, function will launch a three new threads and they will
        /// begin merging the rows in the TestSequence table starting from the first until the last (as Updates)
        /// because the same AppSequenceId will be used.
        /// <para>
        /// This will continue until either the StopMerge button is clicked or all the rows are merged.
        /// </para>
        /// <para>
        /// One thread will merge rows with even AppSequenceId; the other will merge rows with odd id's. 
        /// </para>
        /// <para>
        /// The third thread will merge as inserts (because the AppSequenceId will be new) for 5 rows.
        /// </para>
        /// </summary>
        internal void StartMerge()
        {
            _stop = _stopMerge = false;

            //TestMerge(); 

            Thread tEven = new Thread(Merge);
            tEven.Start(true);
            Thread tOdd = new Thread(Merge);
            tOdd.Start(false);
            Thread tIns = new Thread(Merge);
            tIns.Start();
        }

        /// <summary>
        /// When StartDelete button is clicked, function will launch a two new threads and they will
        /// begin deleting the rows in the TestSequence table starting from the first until the last.
        /// <para>
        /// This will continue until either the StopDelete button is clicked or all the rows are removed.
        /// </para>
        /// <para>
        /// One thread will delete rows with even AppSequenceId; the other will delete rows with odd id's
        /// </para>
        /// </summary>
        internal void StartDelete()
        {
            _stop = _stopDelete = false;

            Thread tEven = new Thread(Delete);
            tEven.Start(true);
            Thread tOdd = new Thread(Delete);
            tOdd.Start(false);

        }

        void Insert()
        {
            DbTableDmlMgr dmlInsert = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                    , DataAccess.Constants.TABLE_TestSequence);
            DbFunctionStructure autogenerate = new DbFunctionStructure();
            if (_daMgr.DatabaseType == DataAccessMgr.EnumDbType.SqlServer
                || _daMgr.DatabaseType == DataAccessMgr.EnumDbType.Db2)
                autogenerate.AutoGenerate = true; // identity column
            else
            { // oracle sequence
                autogenerate.AutoGenerate = false;
                autogenerate.FunctionBody = DataAccess.Constants.SCHEMA_CORE + ".DbSequenceId_Seq.nextVal";
            }

            dmlInsert.AddColumn(Constants.AppSequenceId, _daMgr.BuildParamName(Constants.AppSequenceId));
            dmlInsert.AddColumn(Constants.AppSequenceName, _daMgr.BuildParamName(Constants.AppSequenceName));
            dmlInsert.AddColumn(Constants.AppLocalTime, _daMgr.BuildParamName(Constants.AppLocalTime));
            dmlInsert.AddColumn(Constants.AppSynchTime, _daMgr.BuildParamName(Constants.AppSynchTime));
            dmlInsert.AddColumn(Constants.Remarks, _daMgr.BuildParamName(Constants.Remarks));
            dmlInsert.AddColumn(Constants.ExtraData, _daMgr.BuildParamName(Constants.ExtraData));
            dmlInsert.AddColumn(Constants.DbSequenceId, autogenerate);
            dmlInsert.AddColumn(Constants.DbServerTime, EnumDateTimeLocale.Default); // will default to ddl function

            // generate a compound command
            while (!_stop && !_stopInsert)
            {
                DbCommandMgr dbCmdMgr = new DbCommandMgr(_daMgr);
                DbCommand dbCmd = null;
                for (int i = 0; i < 4; i++)
                {
                    if (dbCmd == null)
                        dbCmd =_daMgr.BuildInsertDbCommand(dmlInsert);

                    else dbCmd = DaMgr.CloneDbCommand(dbCmd);

                    Int64 appSequenceId = _daMgr.GetNextSequenceNumber(Constants.AppSequenceId);

                    Random r = new Random();
                    dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceId)].Value = appSequenceId;
                    dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSequenceName)].Value = DbSetupMgr.GenerateRandomName();
                    dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppLocalTime)].Value = DateTime.Now;
                    dbCmd.Parameters[_daMgr.BuildParamName(Constants.AppSynchTime)].Value = _daMgr.DbSynchTime;
                                        
                    string randomText = "Test String: " + "".PadRight(r.Next(1, 20), 'X'); 
                    dbCmd.Parameters[_daMgr.BuildParamName(Constants.Remarks)].Value = string.Format(
                            "{0}:{1};{2}"
                            , Environment.MachineName
                            , System.Threading.Thread.CurrentThread.ManagedThreadId
                            , randomText);
                    randomText = "Test clob: " + "".PadRight(r.Next(1, 100), 'X');
                    dbCmd.Parameters[_daMgr.BuildParamName(Constants.ExtraData)].Value = randomText;

                    _daMgr.loggingMgr.Trace(string.Format("ThreadID: {0} Adding dbCmd: {1}", i
                                , Thread.CurrentThread.ManagedThreadId)
                                , B1.ILoggingManagement.enumTraceLevel.Level5);

                    dbCmdMgr.AddDbCommand(dbCmd);
                    System.Threading.Thread.Sleep(100);
                }

                _daMgr.loggingMgr.Trace(string.Format("ThreadID: {0} Executing dbCmd"
                            , Thread.CurrentThread.ManagedThreadId)
                            , B1.ILoggingManagement.enumTraceLevel.Level1);

                dbCmdMgr.ExecuteNonQuery();
            }
        }

        void Update( object startOption )
        {
            DbTableStructure dbTable = _daMgr.DbCatalogGetTable( DataAccess.Constants.SCHEMA_CORE
                    , DataAccess.Constants.TABLE_TestSequence );
            Int16 ordinal = dbTable.Columns[ Constants.AppSequenceId ];
            dbTable.Columns.Clear(); // we only want the appSequenceId column
            dbTable.Columns.Add( Constants.AppSequenceId, ordinal );
            PagingMgr pagingMgr = new PagingMgr( _daMgr, dbTable, DataAccess.Constants.PageSize, 20 );
            DbTableDmlMgr dmlUpdate = _daMgr.DbCatalogGetTableDmlMgr( DataAccess.Constants.SCHEMA_CORE
                    , DataAccess.Constants.TABLE_TestSequence );

            bool even = Convert.ToBoolean( startOption );

            dmlUpdate.AddColumn(Constants.Remarks, _daMgr.BuildParamName(Constants.Remarks));
            dmlUpdate.AddColumn(Constants.ExtraData, _daMgr.BuildParamName(Constants.ExtraData));
            dmlUpdate.AddColumn(Constants.AppSynchTime, _daMgr.BuildParamName(Constants.AppSynchTime));
            dmlUpdate.AddColumn(Constants.AppLocalTime, _daMgr.BuildParamName(Constants.AppLocalTime));
            dmlUpdate.AddColumn(Constants.DbServerTime, EnumDateTimeLocale.Default); // will default to ddl function

            dmlUpdate.SetWhereCondition( ( j ) =>
                    j.Column( DataAccess.Constants.TABLE_TestSequence, Constants.AppSequenceId ) ==
                    j.Parameter( DataAccess.Constants.TABLE_TestSequence, Constants.AppSequenceId,
                        _daMgr.BuildParamName( Constants.AppSequenceId ) ) );

            DbCommandMgr dbCmdMgr = new DbCommandMgr( _daMgr );
            DbCommand cmdUpdateOrig = _daMgr.BuildUpdateDbCommand( dmlUpdate );

            while (!_stop && !_stopUpdate)
            {
                DataTable dt = pagingMgr.GetNextPage();
                if (dt != null && dt.Rows.Count == 0)
                    break;  // we have reached the end.

                dbCmdMgr = new DbCommandMgr( _daMgr );
                foreach (DataRow dr in dt.Rows)
                {
                    if (_stop || _stopUpdate)
                        break;
                    Int64 appSeqId = Convert.ToInt64( dr[ Constants.AppSequenceId ] );
                    if ((appSeqId % 2 == 0 && even) // even
                        || (appSeqId % 2 != 0 && !even)) // odd
                    {
                        // perform update
                        DbCommand cmdUpdate = _daMgr.CloneDbCommand( cmdUpdateOrig );

                        cmdUpdate.Parameters[ _daMgr.BuildParamName( Constants.AppSequenceId ) ].Value = appSeqId;
                        cmdUpdate.Parameters[ _daMgr.BuildParamName( Constants.AppLocalTime ) ].Value = DateTime.Now;
                        cmdUpdate.Parameters[ _daMgr.BuildParamName( Constants.AppSynchTime ) ].Value = _daMgr.DbSynchTime;
                        cmdUpdate.Parameters[ _daMgr.BuildParamName( Constants.Remarks ) ].Value
                                = string.Format( "Updated By Thread: {0}; Server: {1}"
                                , System.Threading.Thread.CurrentThread.ManagedThreadId
                                , Environment.MachineName );
                        cmdUpdate.Parameters[ _daMgr.BuildParamName( Constants.ExtraData ) ].Value
                                = _largeObject;
                        dbCmdMgr.AddDbCommand( cmdUpdate );
                    }
                }
                if (!dbCmdMgr.IsNoOpDbCommand)
                    dbCmdMgr.ExecuteNonQuery();
                System.Threading.Thread.Sleep( 100 );
            }
            if (_testStoppedHdlr != null)
                _testStoppedHdlr( DbSetupMgr.TestTypeEnum.Update );
        }

        void Merge( object startOption )
        {
            DbTableStructure dbTable = _daMgr.DbCatalogGetTable( DataAccess.Constants.SCHEMA_CORE
                    , DataAccess.Constants.TABLE_TestSequence );
            Int16 ordinalId = dbTable.Columns[Constants.AppSequenceId];
            Int16 ordinalName = dbTable.Columns[Constants.AppSequenceName];
            dbTable.Columns.Clear(); // 
            dbTable.Columns.Add(Constants.AppSequenceId, ordinalId);
            dbTable.Columns.Add(Constants.AppSequenceName, ordinalName);
            PagingMgr pagingMgr = new PagingMgr( _daMgr, dbTable, DataAccess.Constants.PageSize, 20 );
            DbTableDmlMgr dmlMerge = _daMgr.DbCatalogGetTableDmlMgr( DataAccess.Constants.SCHEMA_CORE
                    , DataAccess.Constants.TABLE_TestSequence );

            bool? even = null;
            if (startOption != null)
                even = Convert.ToBoolean(startOption);

            if (!even.HasValue)
            {
                DbFunctionStructure autogenerate = new DbFunctionStructure();
                if (_daMgr.DatabaseType == DataAccessMgr.EnumDbType.Oracle)
                { // oracle sequence
                    autogenerate.AutoGenerate = true;
                    autogenerate.FunctionBody = DataAccess.Constants.SCHEMA_CORE + ".DbSequenceId_Seq.nextVal";
                    dmlMerge.AddColumn(Constants.DbSequenceId, autogenerate);
                }
            }

            dmlMerge.AddColumn(Constants.AppSequenceId, _daMgr.BuildParamName(Constants.AppSequenceId)
                , DbTableColumnType.ForInsertOnly);
            dmlMerge.AddColumn(Constants.AppSequenceName, _daMgr.BuildParamName(Constants.AppSequenceName)
                , DbTableColumnType.ForInsertOnly);
            dmlMerge.AddColumn(Constants.Remarks, _daMgr.BuildParamName(Constants.Remarks));
            dmlMerge.AddColumn(Constants.ExtraData, _daMgr.BuildParamName(Constants.ExtraData));
            dmlMerge.AddColumn(Constants.AppSynchTime, _daMgr.BuildParamName(Constants.AppSynchTime));
            dmlMerge.AddColumn(Constants.AppLocalTime, _daMgr.BuildParamName(Constants.AppLocalTime));
            dmlMerge.AddColumn(Constants.DbServerTime, EnumDateTimeLocale.Default); // will default to ddl function

            dmlMerge.SetWhereCondition( ( j ) =>
                    j.Column( DataAccess.Constants.TABLE_TestSequence, Constants.AppSequenceId ) ==
                    j.Parameter( DataAccess.Constants.TABLE_TestSequence, Constants.AppSequenceId,
                        _daMgr.BuildParamName( Constants.AppSequenceId ) ) );

            DbCommandMgr dbCmdMgr = new DbCommandMgr( _daMgr );
            DbCommand cmdMergeOrig = _daMgr.BuildMergeDbCommand( dmlMerge );
            
            while (!_stop && !_stopMerge && even.HasValue)
            {
                DataTable dt = pagingMgr.GetNextPage();
                if (dt != null && dt.Rows.Count == 0)
                    break;  // we have reached the end.

                dbCmdMgr = new DbCommandMgr( _daMgr );
                foreach (DataRow dr in dt.Rows)
                {
                    if (_stop || _stopMerge)
                        break;
                    Int64 appSeqId = Convert.ToInt64( dr[ Constants.AppSequenceId ] );
                    if ((appSeqId % 2 == 0 && even.Value) // even
                        || (appSeqId % 2 != 0 && !even.Value)) // odd
                    {
                        // perform update
                        DbCommand cmdMerge = _daMgr.CloneDbCommand( cmdMergeOrig );

                        cmdMerge.Parameters[ _daMgr.BuildParamName( Constants.AppSequenceId ) ].Value = appSeqId;
                        cmdMerge.Parameters[_daMgr.BuildParamName(Constants.AppSequenceName)].Value = dr[Constants.AppSequenceName];
                        cmdMerge.Parameters[_daMgr.BuildParamName(Constants.AppLocalTime)].Value = DateTime.Now;
                        cmdMerge.Parameters[_daMgr.BuildParamName(Constants.AppSynchTime)].Value = _daMgr.DbSynchTime;
                        cmdMerge.Parameters[ _daMgr.BuildParamName( Constants.Remarks ) ].Value
                                = string.Format( "Merge Update By Thread: {0}; Server: {1}"
                                , System.Threading.Thread.CurrentThread.ManagedThreadId
                                , Environment.MachineName );
                        cmdMerge.Parameters[_daMgr.BuildParamName(Constants.ExtraData)].Value
                                = _largeObject;

                        if (_daMgr.DatabaseType == DataAccessMgr.EnumDbType.Oracle)
                        {
                            // Reduce the clob size to 3KB
                            cmdMerge.Parameters[_daMgr.BuildParamName(Constants.ExtraData)].Value
                                    = cmdMerge.Parameters[_daMgr.BuildParamName(Constants.ExtraData)].Value.ToString().Substring(0, 3 * 1024);
                        }

                        dbCmdMgr.AddDbCommand( cmdMerge );
                    }
                }
                if (!dbCmdMgr.IsNoOpDbCommand)
                    dbCmdMgr.ExecuteNonQuery();
                System.Threading.Thread.Sleep( 100 );
            }
            if (!even.HasValue)
            {
                dbCmdMgr = new DbCommandMgr(_daMgr);
                for (int i = 0; i < 5; i++)
                {

                    DbCommand cmdMerge = _daMgr.CloneDbCommand(cmdMergeOrig);

                    Int64 AppSeqId = _daMgr.GetNextSequenceNumber(Constants.AppSequenceId);
                    // cause an insert by creating a new sequence id
                    cmdMerge.Parameters[_daMgr.BuildParamName(Constants.AppSequenceId)].Value
                            = AppSeqId;
                    cmdMerge.Parameters[_daMgr.BuildParamName(Constants.AppSequenceName)].Value = DbSetupMgr.GenerateRandomName();
                    cmdMerge.Parameters[_daMgr.BuildParamName(Constants.AppLocalTime)].Value = DateTime.Now;
                    cmdMerge.Parameters[_daMgr.BuildParamName(Constants.AppSynchTime)].Value = _daMgr.DbSynchTime;
                    cmdMerge.Parameters[_daMgr.BuildParamName(Constants.Remarks)].Value
                            = string.Format("Merge Insert By Thread: {0}; Server: {1}"
                            , System.Threading.Thread.CurrentThread.ManagedThreadId
                            , Environment.MachineName);

                    if (_daMgr.DatabaseType == DataAccessMgr.EnumDbType.Oracle)
                    {
                        // Reduce the clob size to 3KB
                        cmdMerge.Parameters[_daMgr.BuildParamName(Constants.ExtraData)].Value = "Test MERGE INSERT";
                    }
                    else cmdMerge.Parameters[_daMgr.BuildParamName(Constants.ExtraData)].Value = _largeObject;

                    dbCmdMgr.AddDbCommand(cmdMerge);
                }
                dbCmdMgr.ExecuteNonQuery();
            }
            if (_testStoppedHdlr != null)
                _testStoppedHdlr(DbSetupMgr.TestTypeEnum.Merge);
        }

        void Delete( object startOption )
        {
            DbTableStructure dbTable = _daMgr.DbCatalogGetTable(DataAccess.Constants.SCHEMA_CORE
                    , DataAccess.Constants.TABLE_TestSequence);
            Int16 ordinal = dbTable.Columns[Constants.AppSequenceId];
            dbTable.Columns.Clear(); // we only want the appSequenceId column
            dbTable.Columns.Add(Constants.AppSequenceId, ordinal);

            PagingMgr pagingMgr = new PagingMgr(_daMgr, dbTable, DataAccess.Constants.PageSize, 20);
            DbTableDmlMgr dmlDelete = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                    , DataAccess.Constants.TABLE_TestSequence);
            bool even = Convert.ToBoolean(startOption);
            
            dmlDelete.SetWhereCondition((j) =>
                    j.Column(DataAccess.Constants.TABLE_TestSequence, Constants.AppSequenceId) ==
                    j.Parameter(DataAccess.Constants.TABLE_TestSequence, Constants.AppSequenceId,
                        _daMgr.BuildParamName(Constants.AppSequenceId)));

            DbCommand cmdDeleteOrig = _daMgr.BuildDeleteDbCommand(dmlDelete);

            while (!_stop && !_stopDelete)
            {
                DataTable dt = pagingMgr.GetNextPage();
                if (dt != null && dt.Rows.Count == 0)
                    break;  // we have reached the end.
                foreach (DataRow dr in dt.Rows)
                {
                    if (_stop || _stopDelete)
                        break;
                    Int64 appSeqId = Convert.ToInt64(dr[Constants.AppSequenceId]);
                    if ((appSeqId % 2 == 0 && even) // even
                        || (appSeqId % 2 != 0 && !even)) // odd
                    {
                        // perform update
                        DbCommand cmdDelete = _daMgr.CloneDbCommand(cmdDeleteOrig);
                        cmdDelete.Parameters[_daMgr.BuildParamName(Constants.AppSequenceId)].Value = appSeqId;
                        _daMgr.ExecuteNonQuery(cmdDelete, null, null);
                    }
                }
                System.Threading.Thread.Sleep(100);
            }
            if (_testStoppedHdlr != null)
                _testStoppedHdlr(DbSetupMgr.TestTypeEnum.Delete);
        }

        internal void StopInsert()
        {
            _stopInsert = true;
        }

        internal void StopUpdate()
        {
            _stopUpdate = true;
        }

        internal void StopMerge()
        {
            _stopMerge = true;
        }

        internal void StopDelete()
        {
            _stopDelete = true;
        }

        internal void Exit()
        {
            _stop = true;
            StopInsert();
            StopUpdate();
            StopDelete();
        }

        void TestMerge()
        {
            DbTableDmlMgr dmlMerge1 = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
            , "MergeTest");
            dmlMerge1.AddColumn("Id", _daMgr.BuildParamName("Id"));
            dmlMerge1.AddColumn("Commentx", _daMgr.BuildParamName("CommentX"));

            dmlMerge1.SetWhereCondition((j) =>
                    j.Column("MergeTest", "Id") ==
                    j.Parameter("MergeTest", "Id",
                        _daMgr.BuildParamName("Id")));
            DbCommand cmdMergeOrig1 = _daMgr.BuildMergeDbCommand(dmlMerge1);
            cmdMergeOrig1.Parameters[_daMgr.BuildParamName("Id")].Value = _daMgr.GetNextSequenceNumber(Constants.AppSequenceId);
            cmdMergeOrig1.Parameters[_daMgr.BuildParamName("Commentx")].Value = "TestInsert";

            DbCommandMgr dbCmdMgr = new DbCommandMgr(_daMgr);
            dbCmdMgr.AddDbCommand(cmdMergeOrig1);
            dbCmdMgr.ExecuteNonQuery();
        }

        private void TestCrossJoin()
        {
            //DbTableDmlMgr dmlDeleteMgr = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
            //        , Constants.AppStatus
            //        , Constants.AppId
            //        , Constants.StartDateTime);
            //dmlDeleteMgr.AddJoin(tblAppSesssion
            //        , DbTableJoinType.Cross
            //        , null);
            //dmlDeleteMgr.SetWhereCondition((j) => j.Column(Constants.AppId)
            //    == j.Parameter(dmlDeleteMgr.MainTable.SchemaName
            //        , dmlDeleteMgr.MainTable.TableName
            //        , Constants.AppId
            //        , _daMgr.BuildParamName(Constants.AppId))
            //    &&
            //    j.Column(Constants.StatusDateTime)
            //        < j.Function(_daMgr.FormatDateMathSql(EnumDateDiffInterval.Second
            //            , dmlDeleteMgr.Column(Constants.SignonControl, Constants.TimeoutSeconds)
            //            , dmlDeleteMgr.Column(Constants.StatusDateTime))));
            //DbCommand dbCmdDelete = _daMgr.BuildDeleteDbCommand(dmlDeleteMgr);
        }

        private void testSqlServerJoin()
        {
            DbCommandMgr dbCmdMgr = new DbCommandMgr(_daMgr);

            // select s.name as SchemaName
            //     , t.name as TableName
            //     , c.name as ColumnName
            //     , data_type as DataType
            //     , column_id as OrdinalPosition
            //     , Column_Default as ColumnDefault
            //     , isc.is_nullable
            //     , is_rowguidcol as IsRowGuidCol
            //     , is_identity as IsIdentity
            //     , is_computed as IsComputed
            //     , character_maximum_length as CharacterMaximumLength
            //     , numeric_precision as NumericPrecision
            //     , numeric_precision_radix as NumericPrecisionRadix
            //     , numeric_scale as NumericScale
            //     , datetime_precision as DateTimePrecision
            // from sys.tables t
            // inner join sys.schemas s
            //     on s.schema_Id = t.schema_id
            // inner join sys.columns c
            //     on c.object_Id = t.object_id
            // inner join Information_Schema.Columns isc
            //     on isc.Table_Schema = s.name
            //     and isc.Table_Name = t.name
            //     and isc.Column_Name = c.Name
            // where (@SchemaName is null or s.name = @SchemaName)
            // and (@TableName is null or t.Name = @TableName)
            // order by s.name, t.name, c.column_id
            DbParameter paramSchemaName = _daMgr.CreateParameter("SchemaName", 
                    DbType.String, "", 128, ParameterDirection.Input, null);

            DbParameter paramTableName = _daMgr.CreateParameter("TableName", 
                DbType.String, "", 128, ParameterDirection.Input, null);

            DbTableDmlMgr joinSelect = new DbTableDmlMgr(_daMgr, "sys", "tables", "name as TableName");

            string schemasAlias = joinSelect.AddJoin("sys", "schemas", DbTableJoinType.Inner, 
                    (j) => j.Column("schema_id") == j.Column("tables", "schema_id"),
                    "name as TableName");

            string columnsAlias = joinSelect.AddJoin("sys", "columns", DbTableJoinType.Inner, 
                    (j) => j.Column("object_Id") == j.Column("sys", "tables", "object_Id"),
                    "name as ColumnName", "column_id as OrdinalPosition", "is_rowguidcol as IsRowGuidCol",
                    "is_computed as IsComputed", "is_identity as IsIdentity");

            string columnsInformationAlias = joinSelect.AddJoin("information_schema", "columns", DbTableJoinType.Inner, 
                    (j) => j.Column("Table_Schema") == j.Column("schemas", "name")
                    && j.Column("Table_Name") == j.Column("tables", "name")
                    && j.Column("Column_Name") == j.AliasedColumn(columnsAlias, "name"),
                    "data_type", "Column_Default as ColumnDefault", "character_maximum_length as CharacterMaximumLength", 
                    "numeric_precision as NumericPrecision", "numeric_precision_radix as NumericPrecisionRadix", 
                    "numeric_scale as NumericScale", "datetime_precision as DateTimePrecision", "is_nullable",
                    new DbConstValue("1 as Constant1"));

            // case when isc.is_nullable = 'yes' then 1
            // else 0 
            // end as IsNullable
            joinSelect.AddCaseColumn("0", "IsNullable", 
                joinSelect.When( t => t.AliasedColumn(columnsInformationAlias, "is_nullable") == "yes", "1"));

            // where (@SchemaName is null or s.name = @SchemaName)
            // and (@TableName is null or t.Name = @TableName)
            joinSelect.SetWhereCondition( (j) => 
                    (paramSchemaName == null || j.Column("sys", "schemas", "name") == paramSchemaName)
                    && (paramTableName  == null || j.Column("sys", "tables", "name") == paramTableName));
            
            joinSelect.OrderByColumns.Add(1, new DbQualifiedObject<DbIndexColumnStructure>( "sys", "schemas", 
                    _daMgr.BuildIndexColumnAscending("name")));

            joinSelect.OrderByColumns.Add(2, new DbQualifiedObject<DbIndexColumnStructure>("sys", "tables", 
                    _daMgr.BuildIndexColumnAscending("name")));

            DbCommand dbCmd = _daMgr.BuildSelectDbCommand(joinSelect, 1000);
            dbCmd.Parameters[paramSchemaName.ParameterName].Value = null;
            dbCmd.Parameters[paramTableName.ParameterName].Value = null;
            dbCmdMgr.AddDbCommand(dbCmd);

            DataTable tbl = dbCmdMgr.ExecuteDataTable();
        }

        private void testJoinWithCatalog()
        {
            // This is a left join that makes no practical sense. Demonstrates joining by making use 
            // of catalog.
            DbCommandMgr dbCmdMgr = new DbCommandMgr(_daMgr);

            DbTableStructure tableAppConfigSettings = _daMgr.DbCatalogGetTable(DataAccess.Constants.SCHEMA_CORE, 
                    DataAccess.Constants.TABLE_AppConfigSettings);
            DbTableStructure tableAppConfigParameters = _daMgr.DbCatalogGetTable(DataAccess.Constants.SCHEMA_CORE, 
                    "AppConfigParameters");
                        
            DbTableDmlMgr joinSelect = new DbTableDmlMgr(_daMgr, tableAppConfigSettings, "ConfigSetName", "ConfigKey");
            joinSelect.AddJoin(tableAppConfigParameters, DbTableJoinType.LeftOuter, 
                    (j) => j.Column("AppConfigParameters", "ParameterName") == j.Column("AppConfigSettings", "ConfigSetName"),
                    "ParameterName", "ParameterValue", new DbFunction(_daMgr.GetDbTimeAs(EnumDateTimeLocale.Local, "TheTime")));

            joinSelect.SetWhereCondition( (j) =>
                    j.Column("AppConfigSettings", "ConfigSetName") != j.Parameter("AppConfigSettings", "ConfigSetName", 
                        _daMgr.BuildParamName("TestParam")));

            // case
            //     when T1.ConfigSetName < 'b' then 'Less than B'
            //     when T1.ConfigSetName = 'Z' then 'Is Z'
            // else T1.ConfigSetName
            // end as TestCase 
            joinSelect.AddCaseColumn(joinSelect.AliasedColumn(joinSelect.MainTable.TableAlias, "ConfigSetName"), "TestCase", 
                    joinSelect.When(t => t.AliasedColumn(joinSelect.MainTable.TableAlias, "ConfigSetName") < "b" , "'Less than B'"),
                    joinSelect.When(t => t.AliasedColumn(joinSelect.MainTable.TableAlias, "ConfigSetName") == "Z" , "'Is Z'"));

            joinSelect.SelectDistinct = true;

            DbCommand dbCmd = _daMgr.BuildSelectDbCommand(joinSelect, 1000);
            dbCmd.Parameters[_daMgr.BuildParamName("TestParam")].Value = "TEST";
            dbCmdMgr.AddDbCommand(dbCmd);

            DataTable tbl = dbCmdMgr.ExecuteDataTable();
        }

        private void testInAndBetween()
        {
            DbTableDmlMgr appConfigSettings = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE,
                    DataAccess.Constants.TABLE_AppConfigSettings);

            // Where T1.ConfigSetName > 'a' and T1.ConfigSetName BETWEEN 'ALL' and 'ZERO'
            // and T1.ConfigSetName in ('ALL', 'eContracting')
            appConfigSettings.SetWhereCondition( (t) =>
                    t.Column(DataAccess.Constants.TABLE_AppConfigSettings
                        , Configuration.Constants.ConfigSetName) >= "a"
                    && t.Between(DataAccess.Constants.SCHEMA_CORE
                        , DataAccess.Constants.TABLE_AppConfigSettings, Configuration.Constants.ConfigSetName, 
                            _daMgr.BuildParamName("LowParameter"), _daMgr.BuildParamName("HighParameter"))
                    && t.In(DataAccess.Constants.SCHEMA_CORE
                        , DataAccess.Constants.TABLE_AppConfigSettings, Configuration.Constants.ConfigSetName, 2, 
                            _daMgr.BuildParamName("InParam")));
                        
            DbCommand dbCmd = _daMgr.BuildSelectDbCommand(appConfigSettings, 1000);
            dbCmd.Parameters[_daMgr.BuildParamName("LowParameter")].Value = "ALL";
            dbCmd.Parameters[_daMgr.BuildParamName("HighParameter")].Value = "ZERO";
            dbCmd.Parameters[_daMgr.BuildParamName("InParam1")].Value = "ALL";
            dbCmd.Parameters[_daMgr.BuildParamName("InParam2")].Value = "eContracting";

            DataTable tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];
        }

        private void testDelete()
        {
            DbTableDmlMgr appConfigSettings = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE,
                    DataAccess.Constants.TABLE_AppConfigSettings);

            appConfigSettings.SetWhereCondition( (t) =>
                    t.Column(DataAccess.Constants.TABLE_AppConfigSettings, Configuration.Constants.ConfigKey) == "_");

            DbCommand dbCmd = _daMgr.BuildDeleteDbCommand(appConfigSettings);

            int nNumRows = _daMgr.ExecuteNonQuery(dbCmd, null);
        }

        private void testChange()
        {
            DbTableDmlMgr dmlChange = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE,
                    "AppSessionControl");

            string LastModifiedDateTime = "LastModifiedDateTime";
            string LastModifiedUserCode = "LastModifiedUserCode";
            string CheckInSeconds = "CheckInSeconds";

            dmlChange.AddColumn(CheckInSeconds, _daMgr.BuildParamName(CheckInSeconds));
                        
            DbCommand dbCmd = _daMgr.BuildChangeDbCommand(dmlChange, 
                    new DbQualifiedObject<string>(DataAccess.Constants.SCHEMA_CORE, "AppSessionControl", "LastModifiedUserCode"),
                    new DbQualifiedObject<string>(DataAccess.Constants.SCHEMA_CORE, "AppSessionControl", "LastModifiedDateTime"));

            //Values for update
            dbCmd.Parameters[_daMgr.BuildParamName(CheckInSeconds)].Value = 61;
            dbCmd.Parameters[_daMgr.BuildParamName(LastModifiedDateTime, true)].Value = DateTime.Now;
            dbCmd.Parameters[_daMgr.BuildParamName(LastModifiedUserCode, true)].Value = 2;

            //Last mod columns used in where condition
            dbCmd.Parameters[_daMgr.BuildParamName(LastModifiedDateTime)].Value = DBNull.Value;
            dbCmd.Parameters[_daMgr.BuildParamName(LastModifiedUserCode)].Value = DBNull.Value;

            int nNumRows = _daMgr.ExecuteNonQuery(dbCmd, null);
        }

        public void testEntities()
        {
            B1.Utility.DatabaseSetup.Models.SampleDbEntities entities = new B1.Utility.DatabaseSetup.Models.SampleDbEntities();
            B1.Utility.SecondEntities.SampleDbSecondContainer entitiesSecond
                = new SecondEntities.SampleDbSecondContainer();

            var anonClass = new { a= "asdf", b = "bsdf" };

            var resultsJoin1 = from a in entities.AppConfigSettings
                               join b in entitiesSecond.AppConfigParameters on a.ConfigValue equals b.ParameterValue
                          join c in entities.AppSessions on new { name = b.ParameterName, v = b.ParameterValue } 
                                equals new { name = c.MachineName, v = c.AppVersion }
                          where a.ConfigValue != null && a.ConfigValue != anonClass.b || a.ConfigKey.CompareTo("asdf") > 0
                          select new { a.ConfigKey, a.ConfigValue };

            var resultsJoin2 = from a in entities.AppConfigSettings
                               from b in entitiesSecond.AppConfigParameters.Where(p => p.ParameterValue == a.ConfigValue).DefaultIfEmpty()
                               where b.ParameterValue != null
                               select new { a.ConfigKey, a.ConfigValue };

            var resultsJoin3 = from a in entities.AppConfigSettings
                               from b in entitiesSecond.AppConfigParameters
                          where a.ConfigValue == b.ParameterName
                          select new { a.ConfigKey, a.ConfigValue };

            var resultsJoin4 = from a in entities.AppConfigSettings
                               join b in entitiesSecond.AppConfigParameters on a.ConfigValue equals b.ParameterValue into bleft
                              join c in entities.AppSessions on a.ConfigValue equals c.AppVersion into cleft
                              from b in bleft.DefaultIfEmpty()
                              from c in cleft.DefaultIfEmpty()
                               where b.ParameterValue == null
                              select new { a.ConfigKey, a.ConfigValue };

            var resultsJoin5 = from a in entities.AppConfigSettings
                               join b in entitiesSecond.AppConfigParameters on a.ConfigValue equals b.ParameterValue into bleft
                               where a.ConfigValue != null
                               select new { a.ConfigKey, a.ConfigValue };

            var results1 = (from a in entities.AppConfigSettings
                          where (a.ConfigValue != null && a.ConfigKey != "zzzzzz") || a.ConfigKey == "aaa"
                          orderby new { a.ConfigKey, a.ConfigDescription } descending
                          select new { a.ConfigDescription, a.ConfigKey }).Distinct();

            var results2 = from a in entities.AppConfigSettings
                          where (a.ConfigValue != null && a.ConfigKey != anonClass.b) || a.ConfigKey == "aaa"
                          select a;

            string configKeyParam = "asdf";

            var results3 = from a in entities.AppConfigSettings
                          where (a.ConfigValue != null && a.ConfigKey != configKeyParam) || a.ConfigKey == "aaa"
                          select new {a.ConfigValue, keyValue = a.ConfigKey == "a" ? "A" : a.ConfigKey };
                          
            var results4 = from a in entities.AppConfigSettings
                          select new {a.ConfigValue, keyValue = a.ConfigKey == "a" ? "A" : a.ConfigKey };

            var results5 = entities.AppConfigSettings;

            var results6 = entities.AppConfigSettings.Join(entitiesSecond.AppConfigParameters, k => k.ConfigSetName,
                k => k.ParameterName, (o, i) => i);

            var results7 = entities.AppConfigSettings.Where(a => a.ConfigValue != null).SelectMany(a => entitiesSecond.AppConfigParameters.Where(p => p.ParameterValue == a.ConfigValue), (a, p) => a);

            var results8 = entities.AppConfigSettings.SelectMany(a => entitiesSecond.AppConfigParameters.Where(p => p.ParameterValue == a.ConfigValue).DefaultIfEmpty(), (a, p) => a).Where(a => a.ConfigValue != null);

            var resultsGroupBy1 = from a in entities.AppConfigSettings
                                  join b in entitiesSecond.AppConfigParameters on a.ConfigValue equals b.ParameterValue
                                  where a.ConfigValue != null
                                  //group a by new { a.ConfigKey, a.ConfigSetName, a.ConfigValue } into g
                                  group a by a.ConfigKey into g
                                  select new { theKey = g.Key};
                              
            var resultsSelfJoin1 = from a in entities.AppConfigSettings
                                   join b in entities.AppConfigSettings on a.ConfigKey equals b.ConfigKey
                                   select a;

            var resultsSelfJoin2 = entities.AppConfigSettings.Join(entities.AppConfigSettings,
                     a => a.ConfigKey, b => b.ConfigKey, (c, d) => new { d.ConfigKey, c.ConfigSetName })
                     .Join(entities.AppConfigSettings, a=> a.ConfigKey, b => b.ConfigKey, (g, h) => new { h.ConfigKey })
                    .Select( e => e.ConfigKey );

            var resultsOrderBy = entities.AppConfigSettings.Where(a => a.ConfigValue == "value").OrderBy( a => a.ConfigValue);

            string inValue = "A";
            var resultsInClause1 = from a in entities.AppConfigSettings
                                   from b in entities.AppMasters.Where(m => m.Remarks == a.ConfigValue).DefaultIfEmpty()
                                   where new[] { inValue, "B", "C", "D", inValue }.Contains(a.ConfigKey)
                                   && (a.ConfigKey != null || a.ConfigKey == "test")
                                   select a;

            string remarks = "";
            var resultsSubSelect1 = from a in entities.AppConfigSettings
                                   join b in
                                       (from c in entities.AppMasters
                                        where c.Remarks != remarks
                                        select c) on a.ConfigKey equals b.Remarks
                                   where new[] { inValue, "B", "C", "D", inValue }.Contains(a.ConfigKey)
                                   && (a.ConfigKey != null || a.ConfigKey == "test")
                                   select b;


            var resultsSubSelect2 = from a in entities.AppConfigSettings
                                    join b in
                                        (from c in entities.AppMasters
                                         join d in entities.AppSessions on c.Remarks equals d.AppVersion
                                         where c.Remarks != null
                                         select new { c.Remarks, d.AppVersion }) on a.ConfigKey equals b.Remarks
                                    join c in
                                        (from a in entities.AppMasters
                                         join b in
                                             (from a in entities.AppSessions
                                              where a.AppProduct != "asdf"
                                              select new { a.AppVersion, a.AppProduct }) on a.Remarks equals b.AppVersion
                                         where a.Remarks != null
                                         select new { a.AppId, a.AppCode }) on b.Remarks equals c.AppId
                                    where new[] { inValue, "B", "C", "D", inValue }.Contains(a.ConfigKey)
                                    && (a.ConfigKey != null || a.ConfigKey == "test")
                                    select a;

            DbCommand dbCmd;
            DataTable tbl;

            dbCmd = _daMgr.BuildSelectDbCommand(resultsSubSelect1, null);

            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(resultsSubSelect2, null);

            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(resultsGroupBy1, null);

            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(resultsOrderBy, null);

            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(resultsSelfJoin1, null);

            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(resultsSelfJoin2, null);

            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(resultsJoin1, null);

            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(resultsJoin2, null);

            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(resultsJoin3, null);

            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(resultsJoin4, null);

            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(resultsJoin5, null);

            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(results1, null);

            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];
            
            dbCmd = _daMgr.BuildSelectDbCommand(results2, null);
            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(results3, null);
            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            //change the parameter value;
            configKeyParam = "new value";

            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(results4, null);
            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(results5, null);
            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(results6, null);
            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(results7, null);

            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(results8, null);

            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            dbCmd = _daMgr.BuildSelectDbCommand(resultsInClause1, null);

            tbl = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];


        }

        public static void TestDbMultiContext(DataAccessMgr daMgr)
        {
            B1.Utility.DatabaseSetup.Models.SampleDbEntities entities = new B1.Utility.DatabaseSetup.Models.SampleDbEntities();
            B1.Utility.SecondEntities.SampleDbSecondContainer entitiesSecond
                = new B1.Utility.SecondEntities.SampleDbSecondContainer();

            var resultsJoin1 = from a in entities.AppConfigSettings
                              from b in entitiesSecond.AppConfigParameters
                              from c in entities.AppMasters
                              from d in entitiesSecond.AppConfigParameters
                              where a.ConfigValue == b.ParameterName
                              select new { a.ConfigKey, a.ConfigValue };

            var resultsJoin2 = from a in entities.AppConfigSettings
                              join b in entitiesSecond.AppConfigParameters on a.ConfigValue equals b.ParameterValue
                              join c in entities.AppMasters on a.ConfigKey equals c.AppId
                              // from d in entitiesSecond.AppConfigParameters
                              where a.ConfigValue == b.ParameterName
                              select new { a.ConfigKey, a.ConfigValue };

            DbCommand dbCmd = daMgr.BuildSelectDbCommand(resultsJoin1, null);
            DataTable tbl = daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            DbCommand dbCmd2 = daMgr.BuildSelectDbCommand(resultsJoin2, null);
            DataTable tbl2 = daMgr.ExecuteDataSet(dbCmd2, null, null).Tables[0];
        }

        public static void TestPagingMgrWithLINQ(DataAccessMgr daMgr)
        {
            B1.Utility.DatabaseSetup.Models.SampleDbEntities entities = new B1.Utility.DatabaseSetup.Models.SampleDbEntities();

            var resultsJoin1 = from a in entities.TestSequences
                               from b in entities.TestSequences
                               where a.AppSequenceId != b.AppSequenceId && a.AppSequenceName == b.AppSequenceName
                               orderby new { a.AppSequenceName, a.AppSequenceId } ascending
                               select new { a.AppSequenceId, a.AppSequenceName, a.DbSequenceId };
                               //?? select a;

            PagingMgr pagingMgr = new PagingMgr(daMgr, resultsJoin1, DataAccess.Constants.PageSize, 20);
            DataTable dt = pagingMgr.GetNextPage();

        }

        public static void TestPagingMgrEnumerable(DataAccessMgr daMgr)
        {
            B1.Utility.DatabaseSetup.Models.SampleDbEntities entities = new B1.Utility.DatabaseSetup.Models.SampleDbEntities();
            var query = from a in entities.TestSequences
                        orderby new { a.AppSequenceName, a.AppSequenceId }
                        select new { a.AppSequenceId, a.AppSequenceName, a.DbSequenceId };
            PagingMgr testSequenceMgr = new PagingMgr(daMgr, query, DataAccess.Constants.PageSize, 10);

            PagingMgrEnumerator<Models.TestSequence> pagingMgrEnumerator =
                new PagingMgrEnumerator<Models.TestSequence>(testSequenceMgr);

            int count = 0;
            foreach (Models.TestSequence t in pagingMgrEnumerator)
            {
                count++;
            }
        }

        public static PagingMgr CreateSamplePagingMgrJoin(DataAccessMgr daMgr, Int16? pageSize
                , string pagingState = null)
        {
            DbTableDmlMgr dmlJoin = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE,
                    DataAccess.Constants.TABLE_TestSequence);

            // INNER JOIN B1.TestSequence T2 ON T1.AppSequenceId <> T2.AppSequenceId 
            // AND T1.AppSequenceName = T2.AppSequenceName 
            string t2Alias = dmlJoin.AddJoin(DataAccess.Constants.SCHEMA_CORE, DataAccess.Constants.TABLE_TestSequence,
                    DbTableJoinType.Inner,
                    t => t.Column(DataAccess.Constants.SCHEMA_CORE,
                        DataAccess.Constants.TABLE_TestSequence, Constants.AppSequenceId) !=
                        t.Column(Constants.AppSequenceId) &&
                        t.Column(DataAccess.Constants.SCHEMA_CORE,
                        DataAccess.Constants.TABLE_TestSequence, Constants.AppSequenceName) ==
                        t.Column(Constants.AppSequenceName));


            // where T1.AppSequenceId > 1 AND T1.AppSequenceName > 'A'
            dmlJoin.SetWhereCondition(t => t.Column(Constants.AppSequenceId) > 1 &&
                t.Column(Constants.AppSequenceName) > 'A');

            dmlJoin.AddOrderByColumnAscending(Constants.AppSequenceName);
            dmlJoin.AddOrderByColumnAscending(Constants.AppSequenceId);
            return new PagingMgr(daMgr
                    , dmlJoin
                ///        , new List<string> { Constants.AppSequenceName, Constants.AppSequenceId }
                    , DataAccess.Constants.PageSize
                    , pageSize
                    , pagingState);
        }
    }
}
