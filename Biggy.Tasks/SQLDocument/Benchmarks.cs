﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biggy.SQLServer;

namespace Biggy.Perf.SQLDocument {
  class Benchmarks {

    static string _connectionStringName = "chinook";

    public static void Run() {
      Console.WriteLine("===========================================================");
      Console.WriteLine("SQL SERVER - LOAD A BUNCH OF DOCUMENTS INTO A TABLE");
      Console.WriteLine("===========================================================");

      Console.WriteLine("Connecting to SQL Document Store...");

      // Start clean and fresh . . .
      if(Benchmarks.TableExists("clientdocuments")) {
        Benchmarks.DropTable("clientdocuments");
      }

      var _clientDocuments = new SQLDocumentList<ClientDocument>(_connectionStringName);
      _clientDocuments.Clear();
      var sw = new Stopwatch();

      var addRange = new List<ClientDocument>();
      for (int i = 0; i < 10000; i++) {
        addRange.Add(new ClientDocument { 
          LastName = "Conery " + i, 
          FirstName = "Rob", 
          Email = "rob@tekpub.com" });
      }
      sw.Start();
      var inserted = _clientDocuments.AddRange(addRange);
      sw.Stop();
      Console.WriteLine("\t Just inserted {0} as documents in {1} ms", inserted, sw.ElapsedMilliseconds);

      // Start clean and fresh again . . .
      _clientDocuments.Clear();
      addRange.Clear();
      Benchmarks.DropTable("clientdocuments");
      _clientDocuments = new SQLDocumentList<ClientDocument>(_connectionStringName);
      sw.Reset();

      Console.WriteLine("Loading 100,000 documents");
      for (int i = 0; i < 100000; i++) {
        addRange.Add(new ClientDocument {
          LastName = "Conery " + i,
          FirstName = "Rob",
          Email = "rob@tekpub.com"
        });
      }
      sw.Start();
      inserted = _clientDocuments.AddRange(addRange);
      sw.Stop();
      Console.WriteLine("\t Just inserted {0} as documents in {1} ms", inserted, sw.ElapsedMilliseconds);


      //use a DB that has an int PK
      sw.Reset();
      Console.WriteLine("Loading {0}...", inserted);
      sw.Start();
      _clientDocuments.Reload();
      sw.Stop();
      Console.WriteLine("\t Loaded {0} documents from SQL Server in {1} ms", inserted, sw.ElapsedMilliseconds);

      sw.Reset();
      Console.WriteLine("Querying Middle 100 Documents");
      sw.Start();
      var found = _clientDocuments.Where(x => x.ClientDocumentId > 100 && x.ClientDocumentId < 500);
      sw.Stop();
      Console.WriteLine("\t Queried {0} documents in {1}ms", found.Count(), sw.ElapsedMilliseconds);
    }


    static void DropTable(string tableName) {
      string sql = string.Format("DROP TABLE {0}", tableName);
      var Model = new SQLServerTable<dynamic>(_connectionStringName);
      Model.Execute(sql);
    }

    static bool TableExists(string tableName) {
      bool exists = false;
      string select = ""
          + "SELECT * FROM INFORMATION_SCHEMA.TABLES "
          + "WHERE TABLE_SCHEMA = 'dbo' "
          + "AND  TABLE_NAME = '{0}'";
      string sql = string.Format(select, tableName);
      var Model = new SQLServerTable<dynamic>(_connectionStringName);
      var query = Model.Query<dynamic>(sql);
      if (query.Count() > 0) {
        exists = true;
      }
      return exists;
    }
  }
}
