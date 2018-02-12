using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using UrisU.Models;
using URISUtil.DataAccess;
using URISUtil.Logging;
using URISUtil.Response;

namespace UrisU.DataAccess
{
    public class UserDB
    {
        private static User ReadRow(SqlDataReader reader)
        {
            User retVal = new User();
            retVal.Id = (Guid)reader["id"];
            retVal.FirstName = reader["first_name"] as string;
            retVal.LastName = reader["last_name"] as string;
            retVal.Email = reader["email"] as string;
            retVal.UserName = reader["username"] as string;
            retVal.Password = reader["password"] as string;
            retVal.Active = (bool)reader["active"];

            return retVal;
        }

        private static string AllColumnSelect
        {
            get
            {
                return @"
                    [User].[id],
                    [User].[last_name],
                   [User].[email],
                   [User].[username],
                   [User].[password],
                   [User].[active]
                 ";
            }
        }
        private static void FillData(SqlCommand command, User user)
        {
            command.AddParameter("@FirstName", SqlDbType.NVarChar, user.FirstName);
            command.AddParameter("@LastName", SqlDbType.NVarChar, user.LastName);
            command.AddParameter("@Email", SqlDbType.NVarChar, user.Email);
            command.AddParameter("@Username", SqlDbType.NVarChar, user.UserName);
            command.AddParameter("@Password", SqlDbType.NVarChar, user.Password);
            command.AddParameter("@Active", SqlDbType.Bit, user.Active);
        }

        public static List<User> GetAllUsers(ActiveStatusEnum active)
        {
            try
            {
                List<User> retVal = new List<User>();

                using (SqlConnection connection = new SqlConnection(DBFunctions.ConnectionString))
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = String.Format(@"
                        SELECT
                              {0}
                        FROM
                            [user].[User]
                        WHERE
                            (@Active IS NULL OR [user].[User].active = @Active)
", AllColumnSelect);

                    command.Parameters.Add("@Active", SqlDbType.Bit);
                    switch (active)
                    {
                        case ActiveStatusEnum.Active:
                            command.Parameters["@Active"].Value = true;
                            break;
                        case ActiveStatusEnum.Inactive:
                            command.Parameters["@Active"].Value = false;
                            break;
                        case ActiveStatusEnum.All:
                            command.Parameters["@Active"].Value = DBNull.Value;
                            break;


                    }
                    System.Diagnostics.Debug.WriteLine(command.CommandText);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            retVal.Add(ReadRow(reader));
                        }
                    }
                }
                return retVal;
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                throw ErrorResponse.ErrorMessage(HttpStatusCode.BadRequest, ex);

            }
        }

        public static User GetUser(Guid id)
        {
            try
            {
                User retVal = new User();


                using (SqlConnection connection = new SqlConnection(DBFunctions.ConnectionString))
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = String.Format(@"
                        SELECT
                            {0}
                        FROM
                            [user].[User]
                        WHERE
                            [Id] = @Id
", AllColumnSelect);

                    command.AddParameter("@Id", SqlDbType.UniqueIdentifier, id);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            retVal = ReadRow(reader);
                        }
                        else
                        {
                            return null;
                            ErrorResponse.ErrorMessage(HttpStatusCode.NotFound);
                        }
                    }
                }

                return retVal;
            }

            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                throw ErrorResponse.ErrorMessage(HttpStatusCode.NotFound);

            }
        }

        public static User InsertUser(User user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DBFunctions.ConnectionString))
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT INTO [user].[User]
                        (
                        [first_name],
                        [last_name],
                        [email],
                        [username],
                        [password],
                        [active]
                        )
                    VALUES
                        (
                        @FirstName,
                        @LastName,
                        @Email,
                        @UserName,
                        @Password,
                        @Active
                        )
                    ";
                    FillData(command, user);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                return user;
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                throw ErrorResponse.ErrorMessage(HttpStatusCode.BadRequest, ex);
            }
        }

        public static User UpdateUser(User user, Guid id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DBFunctions.ConnectionString))
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = String.Format(@"
                        UPDATE
                            [user].[User]
                        SET
                            [first_name] = @FirstName,
                            [last_name] = @LastName,
                            [email] = @Email,
                            [username] = @UserName,
                            [password]= @Password,
                            [active] = @Active
                       WHERE
                            [Id] = @Id
                ");

                    FillData(command, user);
                    command.AddParameter("@Id", SqlDbType.UniqueIdentifier, id);
                    if (user.Password == "")
                    {
                        return null;
                    }
                    connection.Open();
                    command.ExecuteNonQuery();

                    return GetUser(id);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                throw ErrorResponse.ErrorMessage(HttpStatusCode.BadRequest, ex);
            }
        }

        public static void DeleteUser(Guid id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DBFunctions.ConnectionString))
                {
                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = String.Format(@"
                            UPDATE
                                [user].[User]
                            SET
                                [Active] = 0
                            WHERE
                                [Id] = @Id
                        ");

                    command.AddParameter("@Id", SqlDbType.UniqueIdentifier, id);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                throw ErrorResponse.ErrorMessage(HttpStatusCode.BadRequest, ex);
            }

        }

    }
}