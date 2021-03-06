﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Biggy.Postgres;

namespace Biggy.Perf.PGList
{
  class Benchmarks
  {

    static string _connectionStringName = "chinookPG";
    public static void Run()
    {
      var sw = new Stopwatch();
      Console.WriteLine("===========================================================");
      Console.WriteLine("POSTGRES - SOME FANCY QUERYING");
      Console.WriteLine("===========================================================");


      Console.WriteLine("Loading up Artists from Chinook...");
      sw.Start();
      var _artists = new PGList<Artist>(_connectionStringName, "artist", "artist_id");
      sw.Stop();
      Console.WriteLine("\tLoaded {0} Artist records in {1} ms", _artists.Count(), sw.ElapsedMilliseconds);


      Console.WriteLine("Loading up Albums from Chinook...");
      sw.Reset();
      sw.Start();
      var _albums = new PGList<Album>(_connectionStringName, "album", "album_id");
      sw.Stop();
      Console.WriteLine("\tLoaded {0} Albums in {1} ms", _artists.Count(), sw.ElapsedMilliseconds);


      Console.WriteLine("Loading up tracks from Chinook...");
      sw.Reset();
      sw.Start();
      var _tracks = new PGList<Track>(_connectionStringName, "track", "track_id");
      sw.Stop();
      Console.WriteLine("\tLoaded {0} Tracks in {1} ms", _tracks.Count(), sw.ElapsedMilliseconds);


      Console.WriteLine("Grab the record for AC/DC...");
      sw.Reset();
      sw.Start();
      var acdc = _artists.FirstOrDefault(a => a.Name == "AC/DC");
      sw.Stop();
      Console.WriteLine("\tFound AC/DC from memory in {0} ms", sw.ElapsedMilliseconds);


      Console.WriteLine("Find all the albums by AC/DC ...");
      sw.Reset();
      sw.Start();
      var acdcAlbums = _albums.Where(a => a.ArtistId == acdc.ArtistId);
      sw.Stop();
      Console.WriteLine("\tFound All {0} AC/DC albums from memory in {1} ms", acdcAlbums.Count(), sw.ElapsedMilliseconds);

      Console.WriteLine("Find all the Tracks from Albums by AC/DC ...");
      sw.Reset();
      sw.Start();
      var acdcTracks = from t in _tracks
                       join a in acdcAlbums on t.AlbumId equals a.AlbumId
                       select t;
      sw.Stop();
      Console.WriteLine("\tFound All {0} tracks by ACDC using in-memory JOIN in {1} ms:", acdcTracks.Count(), sw.ElapsedMilliseconds);
      foreach (var track in acdcTracks)
      {
        Console.WriteLine("\t-{0}", track.Name);
      }
      Console.WriteLine(Environment.NewLine);
      Console.WriteLine("===========================================================");
      Console.WriteLine("POSTGRES - BASIC CRUD OPERATIONS");
      Console.WriteLine("===========================================================");


      sw.Reset();
      Console.WriteLine("Loading up customers from Chinook...");
      sw.Start();
      var customers = new PGList<Customer>(_connectionStringName, "customer", "customer_id");
      sw.Stop();
      Console.WriteLine("\tLoaded {0} records in {1}ms", customers.Count(), sw.ElapsedMilliseconds);

      sw.Reset();
      Console.WriteLine("INSERTING a NEW Customer into Chinook...");
      var newCustomer = new Customer() { LastName = "Atten", FirstName = "John", Email = "xivSolutions@example.com" };
      sw.Start();
      customers.Add(newCustomer);
      sw.Stop();
      Console.WriteLine("\tWrote 1 record for a new count of {0} records in {1} ms", customers.Count(), sw.ElapsedMilliseconds);

      sw.Reset();
      Console.WriteLine("UPDATING the new Customer record in Chinook...");
      newCustomer.FirstName = "Fred";
      sw.Start();
      customers.Update(newCustomer);
      sw.Stop();
      Console.WriteLine("\tUpdated 1 record for a new count of {0} records in {1} ms", customers.Count(), sw.ElapsedMilliseconds);

      sw.Reset();
      Console.WriteLine("DELETE the new Customer record in Chinook...");
      sw.Start();
      customers.Remove(newCustomer);
      sw.Stop();
      Console.WriteLine("\tDeleted 1 record for a new count of {0} records in {1} ms", customers.Count(), sw.ElapsedMilliseconds);


      Console.WriteLine(Environment.NewLine);
      Console.WriteLine("===========================================================");
      Console.WriteLine("POSTGRES - BULK INSERTS AND DELETIONS");
      Console.WriteLine("===========================================================");

      Console.WriteLine("Creating Test Table...");
      Benchmarks.SetUpClientTable();

      sw.Reset();
      int INSERT_QTY = 10000;
      Console.WriteLine("BULK INSERTING  {0} client records in Chinook...", INSERT_QTY);
      var _clients = new PGList<Client>(_connectionStringName, "clients", "client_id");

      var inserts = new List<Client>();
      for (int i = 0; i < INSERT_QTY; i++)
      {
        inserts.Add(new Client() { LastName = string.Format("Atten {0}", i.ToString()), FirstName = "John", Email = "xivSolutions@example.com" });
      }
      sw.Start();
      var inserted = _clients.AddRange(inserts);
      sw.Stop();
      Console.WriteLine("\tInserted {0} records in {1} ms", inserted, sw.ElapsedMilliseconds);

      sw.Reset();
      Console.WriteLine("Loading up Bulk inserted CLients from Chinook...");
      sw.Start();
      _clients = new PGList<Client>(_connectionStringName, "clients", "client_id");
      sw.Stop();
      Console.WriteLine("\tLoaded {0} records in {1}ms", _clients.Count(), sw.ElapsedMilliseconds);

      sw.Reset();
      Console.WriteLine("DELETING added records from Chinook...");
      var toRemove = _clients.Where(x => x.Email == "xivSolutions@example.com");
      sw.Start();
      int removed = _clients.RemoveSet(toRemove);
      sw.Stop();
      Console.WriteLine("\tDeleted {0} records in {1}ms", removed, sw.ElapsedMilliseconds);
    }


    // HELPER METHODS:


    static void SetUpClientTable()
    {
      bool exists = Benchmarks.TableExists("clients");
      if (exists)
      {
        Benchmarks.DropTable("clients");
      }
      Benchmarks.CreateClientsTable();
    }


    static void DropTable(string tableName)
    {
      string sql = string.Format("DROP TABLE {0}", tableName);
      var Model = new PGTable<Client>(_connectionStringName);
      Model.Execute(sql);
    }


    static bool TableExists(string tableName)
    {
      bool exists = false;
      string select = ""
          + "SELECT * FROM information_schema.tables "
          + "WHERE table_schema = 'public' "
          + "AND  table_name = '{0}'";
      string sql = string.Format(select, tableName);
      var Model = new PGTable<dynamic>(_connectionStringName);
      var query = Model.Query<Client>(sql);
      if (query.Count() > 0)
      {
        exists = true;
      }
      return exists;
    }


    static void CreateClientsTable()
    {
      string sql = ""
      + "CREATE TABLE clients "
      + "(client_Id serial NOT NULL, "
      + "last_name Text NOT NULL, "
      + "first_name Text NOT NULL, "
      + "email Text NOT NULL, "
      + "CONSTRAINT client_pkey PRIMARY KEY (client_Id))";

      var Model = new PGTable<Client>(_connectionStringName);
      Model.Execute(sql);
    }



  }
}
