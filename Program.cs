using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Azure.Storage.Blobs;

namespace readrecordtestapp
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "";
            string blobConnString = "";
            //Create100DummyRecords(connectionString);

            Console.WriteLine("Fetching 100 records from Sql server");
            List<Employee> employees = FetchRecordsFromSqlServer(connectionString);
            Console.WriteLine("Successfully fetched 100 records from Sql server");

            Console.WriteLine("Creating and uploading text file to azure blob");
            CreateFileAndUploadtoAzureblob(employees, blobConnString);
            Console.WriteLine("Successfully created flat file and upload to azure blob");

            Console.ReadLine();
        }

        private static List<Employee> FetchRecordsFromSqlServer(string connectionString)
        {
            List<Employee> employees = new List<Employee>();

            using (SqlConnection connection =
                       new SqlConnection(connectionString))
            {
                SqlCommand command =
                    new SqlCommand();
                command.Connection = connection;
                command.CommandText = "spGetRecords";
                command.CommandType = System.Data.CommandType.StoredProcedure;

                connection.Open();

                DataTable dataTable = new DataTable();

                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                dataAdapter.Fill(dataTable);

                foreach (DataRow item in dataTable.Rows)
                {
                    Employee employee = new Employee();
                    employee.ID = Convert.ToInt64(item["ID"].ToString());
                    employee.Name = item["Name"].ToString();
                    employee.Salary = Convert.ToDecimal(item["Salary"].ToString());
                    employee.Department = item["DepartmentName"].ToString();
                    employee.JoinDate = Convert.ToDateTime(item["JoinDate"].ToString());

                    employees.Add(employee);
                }
            }

            return employees;
        }

        private static void CreateFileAndUploadtoAzureblob(List<Employee> employees, string blobConnString)
        {
            string filepath = @"C:\Users\anavard\Desktop\priyankatest\";
            string fileName = "employeerecord.txt";
            //create and write records to a text file
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(Path.Combine(filepath, fileName)))
            {
                foreach (Employee emp in employees)
                {
                    file.WriteLine($"Id: {emp.ID} Name: {emp.Name} Department {emp.Department} Salary: {emp.Salary} JoinDate {emp.JoinDate.Date}");
                }
            }

            //Upload to azure blob
            BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnString);

            string containerName = $"employeerecordcontainer{Guid.NewGuid().ToString()}";

            BlobContainerClient containerClient = blobServiceClient.CreateBlobContainer(containerName);

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            using FileStream uploadFileStream = File.OpenRead(Path.Combine(filepath, fileName));
            blobClient.Upload(uploadFileStream, true);
            uploadFileStream.Close();
        }

        private static void Create100DummyRecords(string connectionString)
        {
            List<Employee> employees = new List<Employee>();

            for (int i = 0; i < 100; i++)
            {
                Employee emp = new Employee()
                {
                    Name = $"Priyanka_{i}",
                    DepartmentId = new Random().Next(1, 5),
                    Salary = Convert.ToDecimal(new Random().Next(10000, 55555)),
                    JoinDate = DateTime.Now.AddDays(-1 * new Random().Next(1, 10000))
                };

                employees.Add(emp);
            }

            using (SqlConnection connection =
                       new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (Employee item in employees)
                {
                    using (SqlCommand command =
                        new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "spInsertRecord";
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Name", item.Name);
                        command.Parameters.AddWithValue("@Salary", item.Salary);
                        command.Parameters.AddWithValue("@DepartMentId", item.DepartmentId);
                        command.Parameters.AddWithValue("@JoinDate", item.JoinDate);



                        command.ExecuteNonQuery();
                    }

                }
            }

        }
    }
}
