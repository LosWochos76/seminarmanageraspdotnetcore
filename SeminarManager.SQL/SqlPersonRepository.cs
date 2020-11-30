using System;
using System.Collections.Generic;
using Npgsql;
using SeminarManager.Model;
using SimpleHashing.Net;

namespace SeminarManager.SQL
{
    public class SqlPersonRepository : IPersonRepository
    {
        private ISimpleHash simpleHash = new SimpleHash();
        private NpgsqlConnection connection;

        public SqlPersonRepository(NpgsqlConnection connection)
        {
            this.connection = connection;

            if (!TableExists()) 
            {
                CreateTable();
                Save(new Person() { 
                    Firstname="Alexander", 
                    Lastname="Stuckenholz", 
                    IsAdmin=true, 
                    EMail="alexander.stuckenholz@hshl.de",
                    Password="test"});
            }
        }

        private void CreateTable() 
        {
            string sql = "create table seminarmanager.persons (" +
                "id serial primary key, " +
                "firstname varchar(100) not null, " +
                "lastname varchar(100) not null, " +
                "email varchar(100) not null, " +
                "password_hash varchar(100), " +
                "is_admin boolean not null)";

            using (var cmd = new NpgsqlCommand(sql, connection)) 
            {
                cmd.ExecuteNonQuery();
            }
        }

        private bool TableExists() 
        {
            var sql = "SELECT EXISTS (" +
                "SELECT FROM pg_tables WHERE " +
                "schemaname = 'seminarmanager' and tablename = 'persons')";

            using (var cmd = new NpgsqlCommand(sql, connection))
            {
                return Convert.ToBoolean(cmd.ExecuteScalar());
            }
        }

        private Person FromReader(NpgsqlDataReader reader)
        {
            var obj = new Person();
            obj.ID = reader.GetInt32(0);
            obj.Firstname = reader.GetString(1);
            obj.Lastname = reader.GetString(2);
            obj.EMail = reader.GetString(3);
            obj.Password = reader.GetString(4);
            obj.IsAdmin = reader.GetBoolean(5);
            return obj;
        }

        public List<Person> All(int from = 0, int max = 1000)
        {
            string sql = "SELECT id,firstname,lastname," + 
                "email,password_hash,is_admin FROM seminarmanager.persons " + 
                "limit :max offset :from";

            var result = new List<Person>();
            using (var cmd = new NpgsqlCommand(sql, connection)) 
            {
                cmd.Parameters.AddWithValue(":from", from);
                cmd.Parameters.AddWithValue(":max", max);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var obj = FromReader(reader);
                        result.Add(obj);
                    }
                }
            }

            return result;
        }

        public Person ById(int id)
        {
            string sql = "SELECT id,firstname,lastname," + 
                "email,password_hash,is_admin FROM seminarmanager.persons where id=:id";

            using (var cmd = new NpgsqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue(":id", id);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return FromReader(reader);
                    }
                }
            }

            return null;
        }

        public void Delete(int id)
        {
            string sql = "delete FROM seminarmanager.persons where id=:id";

            using (var cmd = new NpgsqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue(":id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public Person FindAdminByEmailAndPassword(LoginModel login)
        {
            string sql = "SELECT id,firstname,lastname," + 
                "email,password_hash,is_admin FROM seminarmanager.persons where email=:email";

            using (var cmd = new NpgsqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue(":email", login.Email);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var obj = FromReader(reader);
                        if (simpleHash.Verify(login.Password, obj.Password))
                            return obj;
                    }
                }
            }

            return null;
        }

        public void Save(Person obj)
        {
            obj.Password = obj.IsAdmin ? simpleHash.Compute(obj.Password) : string.Empty;

            string sql = "insert into seminarmanager.persons " +
                "(firstname,lastname,email,password_hash,is_admin) values " +
                "(:firstname,:lastname,:email,:password_hash,:is_admin) RETURNING id";

            using (var cmd = new NpgsqlCommand(sql, connection)) 
            {
                cmd.Parameters.AddWithValue(":firstname", obj.Firstname);
                cmd.Parameters.AddWithValue(":lastname", obj.Lastname);
                cmd.Parameters.AddWithValue(":email", obj.EMail);
                cmd.Parameters.AddWithValue(":password_hash", obj.Password);
                cmd.Parameters.AddWithValue(":is_admin", obj.IsAdmin);
                obj.ID = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}