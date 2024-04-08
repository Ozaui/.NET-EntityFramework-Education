// Migration komutları
// dotnet ef database drop --force Tüm databasei siler
// dotnet ef database update 0 Tüm kolonları siler
// dotnet ef migrations remove Son migrationu siler
// dotnet ef database update Deneme Verdiğin isimdeki migration versiyonuna geri döner

// convertion
// data annotations
// fluent api

//Database First 
//String.cs Supplier.cs Shipper.cs SalesReport.cs PurchaseOrderStatus.cs PurchaseOrder.cs PurchaseOrderDetail.cs Privilege.cs OrdersTaxStatus.cs OrdersStatus.cs OrderDetailsStatus.cs OrderDetail.cs Order.cs NorthwindContext.cs Invoice.cs InventoryTransaction.cs InventoryTransactionType.cs Employee.cs Customer.cs Product.cs
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations.Schema;
using ConsoleApp;

namespace MyApp
{
	//Entity Classes
	public class ShopContext : DbContext
	{
		public DbSet<User> Users { get; set; }

		public DbSet<Customer> Customers { get; set; }
		public DbSet<Address> Addresses { get; set; } // Adrese hiçbir zaman direkt olarak ulaşmak 
													  //istemiyor isek buraya tanımlama yapmamamız daha doğru olacaktır.
		public DbSet<Product> Products { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Order> Orders { get; set; }

		public static readonly ILoggerFactory MyLoggerFactory
			= LoggerFactory.Create(builder => { builder.AddConsole(); });
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder
				.UseLoggerFactory(MyLoggerFactory)
				//.UseSqlite("Data Source=shop.db");
				// .UseSqlServer(@"Data Source=.\SQLEXPRESS;Initial Catalog=ShopDb;Integrated Security=SSPI; TrustServerCertificate=True");
				.UseMySql(@"server=localhost;port=3306;database=Shopdb;user=root;password=!YorickPass102030!;", new MySqlServerVersion(new Version(6, 0, 0)));
		}

		//Tekrarlayan kayitlari engelleyip many - to- many iliski icin
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Product_Category>()
			.HasKey(t => new { t.ProductId, t.CategoryId });

			modelBuilder.Entity<Product_Category>()
								.HasOne(pc => pc.Product)
								.WithMany(p => p.product_Categories)
								.HasForeignKey(pc => pc.ProductId);

			modelBuilder.Entity<Product_Category>()
								.HasOne(pc => pc.Category)
								.WithMany(c => c.product_Categories)
								.HasForeignKey(pc => pc.CategoryId);

		}
	}
	public static class DataSeeding
	{
		private static Product[] testProducts =
			{
				new Product(){Name = "`Test Name 1", Price = 1000},
				new Product(){Name = "`Test Name 2", Price = 2000},
				new Product(){Name = "`Test Name 3", Price = 3000},
				new Product(){Name = "`Test Name 4", Price = 4000},
				new Product(){Name = "`Test Name 5", Price = 5000},
				new Product(){Name = "`Test Name 6", Price = 6000},
				new Product(){Name = "`Test Name 7", Price = 7000},
			};
		private static Category[] testCategory =
		{
				new Category(){Name = "`Test Name 1", },
				new Category(){Name = "`Test Name 2", },
				new Category(){Name = "`Test Name 3", },
				new Category(){Name = "`Test Name 4", },
				new Category(){Name = "`Test Name 5", },
				new Category(){Name = "`Test Name 6", },
				new Category(){Name = "`Test Name 7", },
			};
		public static void Seed(DbContext context)
		{

			if (context.Database.GetPendingMigrations().Count() == 0) // Butun migrationlar database e aktarilmis ise
			{
				//ShopContext
				if (context is ShopContext)
				{
					ShopContext _context = context as ShopContext;
					if (_context.Products.Count() == 0)
					{
						//Product ekle\
						_context.Products.AddRange(testProducts);
					}
					if (_context.Categories.Count() == 0)
					{
						//Category ekle
						_context.Categories.AddRange(testCategory);
					}


				}
				//AbcContext
				context.SaveChanges();
			}
		}
	}
	public class User
	{
		[Required]
		public int Id { get; set; }
		[MinLength(8), MaxLength(16)]
		public string UserName { get; set; }
		[Column(TypeName = "varchar(20)")]
		public string Email { get; set; }
		public Customer Customer { get; set; }
		public List<Address> Addresses { get; set; } //Navigation Property
													 //Bir kullanıcının birden fazla adresi olabilir mantığından.
	}
	public class Customer
	{
		public int Id { get; set; }
		public string IdentityNumber { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public User User { get; set; }
		public int UserId { get; set; }

	}
	public class Supplier
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string TextNumber { get; set; }
	}
	public class Address
	{
		public int Id { get; set; }
		public string FullName { get; set; }
		public string Title { get; set; }
		public string Body { get; set; }
		public User user { get; set; } //Bir adres bir kişiye ait olabilir mantığından
		public int UserId { get; set; } //Navigation Property
										//Bunu yazmasak da kendi kendine oluşacak zaten.
										//? nullable değer 
	}
	public class Product
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Otomatik arttirmiyor artik
		public int Id { get; set; }
		[MaxLength(100)]
		[Required]
		public string Name { get; set; }
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)] //Bilgi aktarildiktan sonra bir daha degismeyecek
		public DateTime InsertedDate { get; set; } = DateTime.Now;
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)] //Degistirilebilir alan
		public DateTime LastUpdatedDate { get; set; } = DateTime.Now;

		public decimal Price { get; set; }
		public List<Product_Category> product_Categories { get; set; }

	}
	public class Category
	{
		[Key]
		public int Id { get; set; }
		[MaxLength(100)] // NVARCHAR alan uzunlugu
		[Required] // Zorunlu alan
		public string Name { get; set; }
		public List<Product_Category> product_Categories { get; set; }
	}
	public class Product_Category
	{
		public int ProductId { get; set; }
		public Product Product { get; set; }
		public int CategoryId { get; set; }
		public Category Category { get; set; }

	}
	// [NotMapped] // Database tablosunda yer almayacak
	// [Table("Siparis")] // Ismi degisir
	public class Order
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public DateTime Dateadded { get; set; }

	}
	internal class Program
	{
		static void Main(string[] args)
		{
			using (var db = new NorthwindContext())
			{
				var products = db.Products.ToList();
				foreach (var item in products)
				{
					Console.WriteLine(item.ProductName);
				}
			}


			// DataSeeding.Seed(new ShopContext());


			// //Birebir olduğundan UserIdnin üzerine tekrar veri atamayız.Yani kod bir kere çalışır.
			// using (var db = new ShopContext())
			// {
			// 	// customer tablosuna veri atama
			// 	// var customers = new Customer()
			// 	// {
			// 	// 	IdentityNumber = "123456789",
			// 	// 	FirstName = "Özay",
			// 	// 	LastName = "Yıldız",
			// 	// 	UserId = 2
			// 	// 	// User = db.Users.FirstOrDefault(i => i.Id == 3) ikisi de aynı işi yapıyor
			// 	// };
			// 	// db.Customers.Add(customers);
			// 	// db.SaveChanges();


			// 	// User bilgileri aracılığı ile customer da doldurma

			// 	var user = new User()
			// 	{
			// 		UserName = "deneme",
			// 		Email = "deneme@gmail.com",
			// 		Customer = new Customer()
			// 		{
			// 			FirstName = "Deneme",
			// 			LastName = "Deneme",
			// 			IdentityNumber = "123456789"
			// 		}
			// 	};
			// 	db.Users.Add(user);
			// 	db.SaveChanges();
			// }


			// InsertUsers();
			// InsertAddresses();


			// Id atamadan kişiye veri atama işlemi
			// using (var db = new ShopContext())
			// {
			// 	var user = db.Users.FirstOrDefault(i => i.UserName == "Özay Melih");
			// 	if (user != null)
			// 	{
			// 		user.Addresses = new List<Address>();
			// 		user.Addresses.AddRange(
			// 			new List<Address>(){
			// 			new Address() { FullName = "Özay Melih", Title = "Ev Adres", Body = "Isparta.." },
			// 			new Address() { FullName = "Ozan", Title = "Ev Adres", Body = "Bolu.." },
			// 			new Address() { FullName = "Ayşe", Title = "Ev Adres", Body = "Balıkesir.." },
			// 			new Address() { FullName = "Özkan", Title = "Ev Adres", Body = "İstanbul.." }
			// 			}
			// 		);
			// 		db.SaveChanges();
			// 	}
			// }


			//--İlk metodların kullanımları--
			// AddProduct();
			// AddProducts();
			// GetAllProducts();
			// GetProductById(1);
			// GetProductByName("S6");
			// UpdateProduct();
			// DeleteProduct(2);
		}

		static void InsertUsers()
		{
			var users = new List<User>()
			{
				new User(){UserName = "Özay Melih", Email = "ozay.m.yildiz@gmail.com"},
				new User(){UserName = "Merve", Email = "merve@gmail.com"},
				new User(){UserName = "İlkim", Email = "ilkim@gmail.com"},
				new User(){UserName = "Ozan", Email = "ozan@gmail.com"},
			};
			// using (var db = new ShopContext())
			// {
			// 	db.Users.AddRange(users);
			// 	db.SaveChanges();
			// }
		}
		static void InsertAddresses()
		{
			var addresses = new List<Address>()
			{
				new Address(){FullName = "Özay Melih", Title = "Ev Adres" , Body = "Isparta..", UserId = 1},
				new Address(){FullName = "Merve", Title = "İş Adresş" , Body = "Balıkesir..", UserId = 2},
				new Address(){FullName = "İlkim", Title = "Okul" , Body = "Balıkesir..", UserId = 3},
				new Address(){FullName = "Ozan", Title = "Ev Adres" , Body = "Bolu..", UserId = 3},
				new Address(){FullName = "Ayşe", Title = "Ev Adres" , Body = "Balıkesir..", UserId = 2},
				new Address(){FullName = "Özkan", Title = "Ev Adres" , Body = "İstanbul..", UserId = 4},

			};
			using (var db = new ShopContext())
			{
				db.Addresses.AddRange(addresses);
				db.SaveChanges();
			}
		}



		// ----------------------- Yazdığımız metodlar ---------------------


		// static void DeleteProduct(int id)
		// {
		// 	using (var db = new ShopContext())
		// 	{
		// 		var p = db.Products.FirstOrDefault(i => i.Id == id);
		// 		if (p != null)
		// 		{
		// 			db.Products.Remove(p);
		// 			db.SaveChanges();
		// 			Console.WriteLine("Ürün Silindi");
		// 		}
		// 	}
		// }
		// static void UpdateProduct()
		// {
		// 	using (var db = new ShopContext())
		// 	{
		// 		var p = db.Products.Where(i => i.Id == 1).FirstOrDefault();

		// 		if (p != null)
		// 		{
		// 			p.Price = 2400;
		// 			db.Products.Update(p);
		// 			db.SaveChanges();
		// 		}
		// 	}
		// 	// using (var db = new ShopContext())
		// 	// {
		// 	// 	var entity = new Product() { Id = 1 };
		// 	// 	db.Products.Attach(entity);
		// 	// 	entity.Price = 3000;
		// 	// 	db.SaveChanges();
		// 	// }

		// 	// using (var db = new ShopContext())
		// 	// {
		// 	// 	var p = db.Products.AsNoTracking().Where(i => i.Id == 1).FirstOrDefault();
		// 	// 	if (p != null)
		// 	// 	{
		// 	// 		p.Price *= 1.2m;
		// 	// 		db.SaveChanges();
		// 	// 		Console.WriteLine("Update yapıldı");
		// 	// 	}
		// 	// }
		// }
		// static void GetProductByName(string name)
		// {
		// 	using (var context = new ShopContext())
		// 	{
		// 		var products = context.Products
		// 		.Where(p => p.Name.ToLower().Contains(name.ToLower()))
		// 		.Select(p => new
		// 		{
		// 			p.Name,
		// 			p.Price
		// 		})
		// 		.ToList();
		// 		foreach (var item in products)
		// 		{
		// 			Console.WriteLine($"Name: {item.Name}, Price: {item.Price}");
		// 		}

		// 	}
		// }
		// static void GetProductById(int id)
		// {
		// 	using (var context = new ShopContext())
		// 	{
		// 		var product = context.Products
		// 		.Where(p => p.Id == id)
		// 		.Select(p => new
		// 		{
		// 			p.Name,
		// 			p.Price
		// 		})
		// 		.FirstOrDefault();
		// 		Console.WriteLine($"Name: {product.Name}, Price: {product.Price}");

		// 	}
		// }
		// static void GetAllProducts()
		// {
		// 	using (var context = new ShopContext())
		// 	{
		// 		var products = context
		// 		.Products
		// 		.Select(product =>
		// 			new
		// 			{
		// 				product.Name,
		// 				product.Price
		// 			}
		// 		)
		// 		.ToList();
		// 		foreach (var item in products)
		// 		{
		// 			Console.WriteLine($"Name: {item.Name}, Price: {item.Price}");
		// 		}
		// 	}
		// }
		// static void AddProduct()
		// {
		// 	using (var db = new ShopContext())
		// 	{
		// 		var product = new Product { Name = "Samsung S5", Price = 2000 };

		// 		db.Products.Add(product);
		// 		db.SaveChanges();
		// 	}
		// }
		// static void AddProducts()
		// {
		// 	using (var db = new ShopContext())
		// 	{
		// 		var products = new List<Product>
		// 	{
		// 		new Product{ Name = "Samsung S5", Price = 2000},
		// 		new Product{ Name = "Samsung S6", Price = 3000},
		// 		new Product{ Name = "Samsung S7", Price = 4000},
		// 		new Product{ Name = "Samsung S8", Price = 5000},
		// 		new Product{ Name = "Samsung S9", Price = 6000},
		// 	};
		// 		// foreach (var item in products)
		// 		// {
		// 		// 	db.Products.Add(item);
		// 		// }
		// 		db.Products.AddRange(products);
		// 		db.SaveChanges();
		// 	}
		//}
	}
}
