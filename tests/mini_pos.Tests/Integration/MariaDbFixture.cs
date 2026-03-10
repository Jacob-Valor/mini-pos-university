using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using mini_pos.Services;
using MySqlConnector;
using Xunit;

namespace mini_pos.Tests.Integration;

public sealed class MariaDbFixture : IAsyncLifetime
{
    private const ushort MariaDbPort = 3306;

    private const string Image = "mariadb:10.11";
    private const string Database = "mini_pos_test";

    private const string DbUsername = "test_user";
    private const string DbPassword = "test_password";
    private const string RootPassword = "root_password";

    private readonly TestcontainersContainer _container;

    public string ConnectionString { get; private set; } = string.Empty;
    public IMySqlConnectionFactory ConnectionFactory { get; private set; } = null!;

    public string SeedCustomerId { get; } = "CUS0000001";
    public string SeedEmployeeId { get; } = "EMP001";
    public string SeedEmployeeUsername { get; } = "user1";
    public string SeedBrandId { get; } = "B001";
    public string SeedCategoryId { get; } = "C001";
    public int SeedExchangeRateId { get; } = 1;

    public MariaDbFixture()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED")))
        {
            Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_DISABLED", "true");
        }

        _container = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage(Image)
            .WithName($"mini-pos-mariadb-{Guid.NewGuid():N}")
            .WithPortBinding(MariaDbPort, true)
            // MariaDB accepts both MARIADB_* and MYSQL_* variables; we set both for compatibility.
            .WithEnvironment("MARIADB_ROOT_PASSWORD", RootPassword)
            .WithEnvironment("MARIADB_DATABASE", Database)
            .WithEnvironment("MARIADB_USER", DbUsername)
            .WithEnvironment("MARIADB_PASSWORD", DbPassword)
            .WithEnvironment("MYSQL_ROOT_PASSWORD", RootPassword)
            .WithEnvironment("MYSQL_DATABASE", Database)
            .WithEnvironment("MYSQL_USER", DbUsername)
            .WithEnvironment("MYSQL_PASSWORD", DbPassword)
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var host = _container.Hostname;
        var port = _container.GetMappedPublicPort(MariaDbPort);

        ConnectionString = $"Server={host};Port={port};Database={Database};User={DbUsername};Password={DbPassword};SslMode=None;Pooling=false;";
        ConnectionFactory = new MySqlConnectionFactory(ConnectionString);

        await WaitForDatabaseReadyAsync(ConnectionString, TimeSpan.FromSeconds(60));

        await ApplySchemaAndSeedAsync(ConnectionString);
    }

    public Task DisposeAsync()
    {
        return _container.DisposeAsync().AsTask();
    }

    private async Task ApplySchemaAndSeedAsync(string connectionString)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        foreach (var statement in GetSchemaStatements())
        {
            await using var command = new MySqlCommand(statement, connection);
            await command.ExecuteNonQueryAsync();
        }

        foreach (var statement in GetSeedStatements())
        {
            await using var command = new MySqlCommand(statement, connection);
            await command.ExecuteNonQueryAsync();
        }
    }

    private static async Task WaitForDatabaseReadyAsync(string connectionString, TimeSpan timeout)
    {
        var start = DateTime.UtcNow;

        while (DateTime.UtcNow - start < timeout)
        {
            try
            {
                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                await using var command = new MySqlCommand("SELECT 1", connection);
                await command.ExecuteScalarAsync();
                return;
            }
            catch
            {
                await Task.Delay(500);
            }
        }

        throw new TimeoutException("MariaDB container started but did not become ready in time.");
    }

    private IEnumerable<string> GetSchemaStatements()
    {
        yield return "SET FOREIGN_KEY_CHECKS = 0";
        yield return "DROP TABLE IF EXISTS sales_product";
        yield return "DROP TABLE IF EXISTS sales";
        yield return "DROP TABLE IF EXISTS product";
        yield return "DROP TABLE IF EXISTS supplier";
        yield return "DROP TABLE IF EXISTS category";
        yield return "DROP TABLE IF EXISTS brand";
        yield return "DROP TABLE IF EXISTS customer";
        yield return "DROP TABLE IF EXISTS exchange_rate";
        yield return "DROP TABLE IF EXISTS employee";
        yield return "DROP TABLE IF EXISTS villages";
        yield return "DROP TABLE IF EXISTS districts";
        yield return "DROP TABLE IF EXISTS provinces";
        yield return "SET FOREIGN_KEY_CHECKS = 1";

        yield return @"
CREATE TABLE provinces (
  provid CHAR(2) NOT NULL,
  provname VARCHAR(50) NOT NULL,
  PRIMARY KEY (provid)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

        yield return @"
CREATE TABLE districts (
  distid VARCHAR(10) NOT NULL,
  distname VARCHAR(50) NOT NULL,
  provid CHAR(2) NOT NULL,
  PRIMARY KEY (distid),
  KEY provid (provid),
  CONSTRAINT fk_dist_prov FOREIGN KEY (provid) REFERENCES provinces (provid)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

        yield return @"
CREATE TABLE villages (
  vid VARCHAR(10) NOT NULL,
  vname VARCHAR(100) NOT NULL,
  distid VARCHAR(10) NOT NULL,
  PRIMARY KEY (vid),
  KEY distid (distid),
  CONSTRAINT fk_villa__dist FOREIGN KEY (distid) REFERENCES districts (distid)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

        yield return @"
CREATE TABLE employee (
  emp_id VARCHAR(8) NOT NULL,
  emp_name VARCHAR(50) NOT NULL,
  emp_lname VARCHAR(50) NOT NULL,
  gender VARCHAR(10) NOT NULL,
  date_of_b DATE NOT NULL,
  village_id VARCHAR(10) NOT NULL,
  tel VARCHAR(15) NOT NULL,
  start_date DATE NOT NULL,
  picture LONGBLOB NULL,
  username VARCHAR(25) NOT NULL,
  password VARCHAR(255) NOT NULL,
  status VARCHAR(10) NOT NULL,
  PRIMARY KEY (emp_id),
  UNIQUE KEY uq_employee_username (username),
  KEY village_id (village_id),
  CONSTRAINT employee_ibfk_1 FOREIGN KEY (village_id) REFERENCES villages (vid) ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

        yield return @"
CREATE TABLE customer (
  cus_id VARCHAR(10) NOT NULL,
  cus_name VARCHAR(50) NOT NULL,
  cus_lname VARCHAR(50) NOT NULL,
  gender VARCHAR(10) NOT NULL,
  address VARCHAR(255) NOT NULL,
  tel VARCHAR(15) NOT NULL,
  PRIMARY KEY (cus_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

        yield return @"
CREATE TABLE brand (
  brand_id VARCHAR(4) NOT NULL,
  brand_name VARCHAR(50) NOT NULL,
  PRIMARY KEY (brand_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

        yield return @"
CREATE TABLE category (
  category_id VARCHAR(4) NOT NULL,
  category_name VARCHAR(50) NOT NULL,
  PRIMARY KEY (category_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

        yield return @"
CREATE TABLE supplier (
  sup_id VARCHAR(10) NOT NULL,
  sup_name VARCHAR(100) NOT NULL,
  contract_name VARCHAR(100) NULL,
  email VARCHAR(100) NULL,
  telephone VARCHAR(20) NULL,
  address VARCHAR(255) NULL,
  PRIMARY KEY (sup_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

        yield return @"
CREATE TABLE product (
  barcode VARCHAR(13) NOT NULL,
  product_name VARCHAR(50) NOT NULL,
  unit VARCHAR(25) NOT NULL,
  quantity INT NOT NULL,
  quantity_min INT NOT NULL,
  cost_price DECIMAL(10,0) NOT NULL,
  retail_price DECIMAL(10,0) NOT NULL,
  brand_id VARCHAR(4) NOT NULL,
  category_id VARCHAR(4) NOT NULL,
  status VARCHAR(10) NOT NULL,
  PRIMARY KEY (barcode),
  KEY brand_id (brand_id),
  KEY category_id (category_id),
  CONSTRAINT product_ibfk_1 FOREIGN KEY (brand_id) REFERENCES brand (brand_id) ON UPDATE CASCADE,
  CONSTRAINT product_ibfk_2 FOREIGN KEY (category_id) REFERENCES category (category_id) ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

        yield return @"
CREATE TABLE exchange_rate (
  id INT NOT NULL AUTO_INCREMENT,
  dolar DECIMAL(7,2) NOT NULL,
  bath DECIMAL(6,2) NOT NULL,
  ex_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP(),
  PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

        yield return @"
CREATE TABLE sales (
  sale_id INT NOT NULL AUTO_INCREMENT,
  ex_id INT NOT NULL,
  cus_id VARCHAR(10) NOT NULL,
  emp_id VARCHAR(8) NOT NULL,
  date_sale DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP(),
  subtotal DECIMAL(12,2) NOT NULL,
  pay DECIMAL(12,2) NOT NULL,
  money_change DECIMAL(12,2) NOT NULL,
  PRIMARY KEY (sale_id),
  KEY cus_id (cus_id),
  KEY emp_id (emp_id),
  KEY ex_id (ex_id),
  CONSTRAINT sales_ibfk_1 FOREIGN KEY (cus_id) REFERENCES customer (cus_id) ON UPDATE CASCADE,
  CONSTRAINT sales_ibfk_2 FOREIGN KEY (emp_id) REFERENCES employee (emp_id) ON UPDATE CASCADE,
  CONSTRAINT sales_ibfk_3 FOREIGN KEY (ex_id) REFERENCES exchange_rate (id) ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

        yield return @"
CREATE TABLE sales_product (
  id INT NOT NULL AUTO_INCREMENT,
  sales_id INT NOT NULL,
  product_id VARCHAR(13) DEFAULT NULL,
  qty INT NOT NULL,
  price DECIMAL(12,2) NOT NULL,
  total DECIMAL(12,2) NOT NULL,
  PRIMARY KEY (id),
  KEY sales_id (sales_id),
  KEY product_id (product_id),
  CONSTRAINT sales_product_ibfk_1 FOREIGN KEY (sales_id) REFERENCES sales (sale_id) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT sales_product_ibfk_2 FOREIGN KEY (product_id) REFERENCES product (barcode) ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    }

    private IEnumerable<string> GetSeedStatements()
    {
        yield return "INSERT INTO provinces (provid, provname) VALUES ('01', 'Test Province')";
        yield return "INSERT INTO districts (distid, distname, provid) VALUES ('0101', 'Test District', '01')";
        yield return "INSERT INTO villages (vid, vname, distid) VALUES ('010101', 'Test Village', '0101')";

        yield return $@"INSERT INTO employee
 (emp_id, emp_name, emp_lname, gender, date_of_b, village_id, tel, start_date, picture, username, password, status)
 VALUES
 ('{SeedEmployeeId}', 'Test', 'User', 'M', '2000-01-01', '010101', '0200000000', '2020-01-01', NULL, '{SeedEmployeeUsername}', 'initial_hash', 'Admin')";

        yield return $@"INSERT INTO customer
 (cus_id, cus_name, cus_lname, gender, address, tel)
 VALUES
 ('{SeedCustomerId}', 'Test', 'Customer', 'M', 'Somewhere', '0200000000')";

        yield return $@"INSERT INTO brand (brand_id, brand_name) VALUES ('{SeedBrandId}', 'Test Brand')";
        yield return $@"INSERT INTO category (category_id, category_name) VALUES ('{SeedCategoryId}', 'Test Category')";

        yield return "INSERT INTO product (barcode, product_name, unit, quantity, quantity_min, cost_price, retail_price, brand_id, category_id, status) " +
                     "VALUES ('P000000000001', 'Seed Product', 'pcs', 100, 1, 1, 2, 'B001', 'C001', 'ມີ')";

        yield return $@"INSERT INTO exchange_rate (id, dolar, bath, ex_date)
 VALUES ({SeedExchangeRateId}, 20000.00, 600.00, '2000-01-01 00:00:00')";
    }
}
