using System.Data.SQLite;
using System.IO;
using System.Web.Mvc;

namespace Gosocket.Dian.Web.App_Start
{

    public class LoggingRazorViewEngine : RazorViewEngine
    {
        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            if (controllerContext.HttpContext.Request.Url.Host == "localhost")
            {
                CreateDatabase();
                InsertRecords(partialPath);
            }
            return base.CreatePartialView(controllerContext, partialPath);
        }

        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            if (controllerContext.HttpContext.Request.Url.Host == "localhost")
            {
                CreateDatabase();
                InsertRecords(viewPath);
            }
            return base.CreateView(controllerContext, viewPath, masterPath);
        }

        private void CreateDatabase()
        {
            if (!File.Exists("MyDatabase.sqlite"))
            {
                SQLiteConnection.CreateFile("MyDatabase.sqlite");
                SQLiteConnection m_dbConnectionCreate;
                m_dbConnectionCreate = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;");
                m_dbConnectionCreate.Open();
                string sqlCreate = "create table logs (id varchar(20), path varchar(255))";
                SQLiteCommand commandCreate = new SQLiteCommand(sqlCreate, m_dbConnectionCreate);
                commandCreate.ExecuteNonQuery();
                m_dbConnectionCreate.Close();
            }
        }

        private void InsertRecords(string viewPath)
        {
            //Select Sqlite
            using (SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;"))
            {
                m_dbConnection.Open();
                bool readerHasRows = false;
                string sql = $"select * from logs where id = '{viewPath.GetHashCode()}' ";
                using (SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            readerHasRows = true;
                        }

                    }
                }

                if (readerHasRows)
                {
                    sql = $"insert into logs (id, path) values ('{viewPath.GetHashCode()}', '{viewPath.Replace('~', ' ').Trim()}')";
                    using (SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection))
                    {
                        command.ExecuteNonQuery();
                    } 
                }
            }


            //SQLiteConnection m_dbConnection;
            //m_dbConnection = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;");
            //m_dbConnection.Open();

            //string sql = $"select * from logs where id = '{viewPath.GetHashCode()}' ";
            //SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            //SQLiteDataReader reader = command.ExecuteReader();
            //if (!reader.HasRows)
            //{
            //    sql = $"insert into logs (id, path) values ('{viewPath.GetHashCode()}', '{viewPath.Replace('~', ' ').Trim()}')";
            //    command = new SQLiteCommand(sql, m_dbConnection);
            //    command.ExecuteNonQuery();
            //}

            //m_dbConnection.Close();
        }

    }
}