namespace mini_pos.Services;

internal static class SqlQueries
{
    public const string ConnectionTest = "SELECT 1";

    public const string EmployeeByUsername = @"
                SELECT e.emp_id, e.emp_name, e.emp_lname, e.gender, e.date_of_b,
                       e.village_id, e.tel, e.start_date, e.username, e.status,
                       v.vname as village_name, d.distname as district_name, p.provname as province_name,
                       d.distid as district_id, p.provid as province_id
                FROM employee e
                LEFT JOIN villages v ON e.village_id = v.vid
                LEFT JOIN districts d ON v.distid = d.distid
                LEFT JOIN provinces p ON d.provid = p.provid
                WHERE e.username = @username";

    public const string Employees = @"
                SELECT e.emp_id, e.emp_name, e.emp_lname, e.gender, e.date_of_b,
                       e.tel, e.start_date, e.username, e.status,
                       v.vname as village_name, d.distname as district_name, p.provname as province_name,
                       v.vid as village_id, d.distid as district_id, p.provid as province_id
                FROM employee e
                LEFT JOIN villages v ON e.village_id = v.vid
                LEFT JOIN districts d ON v.distid = d.distid
                LEFT JOIN provinces p ON d.provid = p.provid
                ORDER BY e.emp_id";

    public const string Products = @"
                SELECT p.barcode, p.product_name, p.unit, p.quantity, p.quantity_min,
                       p.cost_price, p.retail_price, p.status,
                       p.brand_id, p.category_id,
                       b.brand_name, c.category_name
                FROM product p
                LEFT JOIN brand b ON p.brand_id = b.brand_id
                LEFT JOIN category c ON p.category_id = c.category_id
                ORDER BY p.barcode";

    public const string ProductByBarcode = @"
                SELECT p.barcode, p.product_name, p.unit, p.quantity, p.quantity_min,
                       p.cost_price, p.retail_price, p.status,
                       p.brand_id, p.category_id,
                       b.brand_name, c.category_name
                FROM product p
                LEFT JOIN brand b ON p.brand_id = b.brand_id
                LEFT JOIN category c ON p.category_id = c.category_id
                WHERE p.barcode = @barcode";

    public const string Customers = @"
                SELECT cus_id, cus_name, cus_lname, gender, address, tel
                FROM customer
                ORDER BY cus_id";

    public const string Brands = "SELECT brand_id, brand_name FROM brand ORDER BY brand_id";
    public const string ProductTypes = "SELECT category_id, category_name FROM category";

    public const string LatestExchangeRate = "SELECT id, dolar, bath, ex_date FROM exchange_rate ORDER BY ex_date DESC LIMIT 1";
    public const string ExchangeRateHistory = "SELECT id, dolar, bath, ex_date FROM exchange_rate ORDER BY ex_date DESC LIMIT 50";

    public const string ProductExists = "SELECT COUNT(1) FROM product WHERE barcode = @id";
    public const string InsertProduct = @"
                INSERT INTO product (barcode, product_name, unit, quantity, quantity_min, cost_price, retail_price, brand_id, category_id, status)
                VALUES (@id, @name, @unit, @qty, @min, @cost, @price, @brand, @type, @status)";
    public const string UpdateProduct = @"
                UPDATE product SET
                    product_name=@name, unit=@unit, quantity=@qty, quantity_min=@min,
                    cost_price=@cost, retail_price=@price, brand_id=@brand, category_id=@type, status=@status
                WHERE barcode=@id";
    public const string DeleteProduct = "DELETE FROM product WHERE barcode = @id";

    public const string InsertCustomer = @"
                INSERT INTO customer (cus_id, cus_name, cus_lname, gender, address, tel)
                VALUES (@id, @name, @surname, @gender, @addr, @tel)";
    public const string UpdateCustomer = @"
                UPDATE customer SET
                    cus_name=@name, cus_lname=@surname, gender=@gender, address=@addr, tel=@tel
                WHERE cus_id=@id";
    public const string DeleteCustomer = "DELETE FROM customer WHERE cus_id = @id";
    public const string SearchCustomers = @"
                SELECT cus_id, cus_name, cus_lname, gender, address, tel
                FROM customer
                WHERE cus_id LIKE @kw OR cus_name LIKE @kw OR tel LIKE @kw
                LIMIT 20";

    public const string InsertBrand = "INSERT INTO brand (brand_id, brand_name) VALUES (@id, @name)";
    public const string UpdateBrand = "UPDATE brand SET brand_name = @name WHERE brand_id = @id";
    public const string DeleteBrand = "DELETE FROM brand WHERE brand_id = @id";

    public const string InsertProductType = "INSERT INTO category (category_id, category_name) VALUES (@id, @name)";
    public const string UpdateProductType = "UPDATE category SET category_name = @name WHERE category_id = @id";
    public const string DeleteProductType = "DELETE FROM category WHERE category_id = @id";

    public const string Provinces = "SELECT provid, provname FROM provinces ORDER BY provname";
    public const string DistrictsByProvince = "SELECT distid, distname, provid FROM districts WHERE provid = @pid ORDER BY distname";
    public const string VillagesByDistrict = "SELECT vid, vname, distid FROM villages WHERE distid = @did ORDER BY vname";

    public const string InsertEmployee = @"
                INSERT INTO employee
                (emp_id, emp_name, emp_lname, gender, date_of_b, village_id, tel, start_date, username, password, status)
                VALUES
                (@id, @name, @surname, @gender, @dob, @vid, @tel, @start, @user, @pass, @status)";
    public const string UpdateEmployee = @"
                UPDATE employee SET
                    emp_name=@name, emp_lname=@surname, gender=@gender, date_of_b=@dob,
                    village_id=@vid, tel=@tel, status=@status
                WHERE emp_id=@id";
    public const string DeleteEmployee = "DELETE FROM employee WHERE emp_id = @id";
    public const string UpdateEmployeeProfile = @"
                UPDATE employee
                SET emp_name = @name,
                    emp_lname = @surname,
                    gender = @gender,
                    date_of_b = @dob,
                    tel = @tel,
                    username = @username
                WHERE emp_id = @id";
    public const string UpdateEmployeePassword = "UPDATE employee SET password = @pwd WHERE emp_id = @id";
    public const string StoredPasswordHash = "SELECT password FROM employee WHERE username = @username";

    public const string Suppliers = "SELECT sup_id, sup_name, contract_name, email, telephone, address FROM supplier ORDER BY sup_id";
    public const string InsertSupplier = @"
                INSERT INTO supplier (sup_id, sup_name, contract_name, email, telephone, address)
                VALUES (@id, @name, @contact, @email, @tel, @addr)";
    public const string UpdateSupplier = @"
                UPDATE supplier SET
                    sup_name=@name, contract_name=@contact, email=@email, telephone=@tel, address=@addr
                WHERE sup_id=@id";
    public const string DeleteSupplier = "DELETE FROM supplier WHERE sup_id = @id";

    public const string InsertExchangeRate = "INSERT INTO exchange_rate (dolar, bath, ex_date) VALUES (@usd, @thb, @date)";

    public const string InsertSale = @"
                INSERT INTO sales (ex_id, cus_id, emp_id, date_sale, subtotal, pay, money_change)
                VALUES (@exId, @cusId, @empId, @date, @sub, @pay, @change);
                SELECT LAST_INSERT_ID();";
    public const string InsertSaleDetail = @"
                INSERT INTO sales_product (sales_id, product_id, qty, price, total)
                VALUES (@saleId, @prodId, @qty, @price, @total)";
    public const string UpdateStock = @"
                UPDATE product
                SET quantity = quantity - @qty
                WHERE barcode = @prodId AND quantity >= @qty";

    public const string SalesReport = @"
                SELECT sp.product_id, p.product_name, p.unit,
                       SUM(sp.qty) as total_qty,
                       sp.price,
                       SUM(sp.total) as total_amount
                FROM sales s
                JOIN sales_product sp ON s.sales_id = sp.sales_id
                JOIN product p ON sp.product_id = p.barcode
                WHERE s.date_sale >= @start AND s.date_sale < @endExclusive
                GROUP BY sp.product_id, sp.price
                ORDER BY p.product_name";
}
