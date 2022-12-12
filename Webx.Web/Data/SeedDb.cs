using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Helpers;

namespace Webx.Web.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public SeedDb(DataContext context,IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();

            await AddStatusesAsync();
            await CheckCreatedRoles();
            await AddAdminsAsync();
            await AddTechniciansAsync();
            await AddProductManagersAsync();
            await AddCustomersAsync();
            await AddBrandsAsync();
            await AddCategoriesAsync();
            await AddProductsAsync();
            await AddServicesAsync();
            await AddStoresAsync();
            await AddStocksAsync();
            await AddAppointmentsAsync();
            await AddOrdersAsync();
            await AddOrdersDetailsAsync();

        }

        private async Task AddOrdersDetailsAsync()
        {
            if (!_context.OrderDetails.Any())
            {
                var appointmentOrders = await _context.Orders.Include(o => o.Appointment).Where(o => o.Appointment != null).ToListAsync();
                int increment = 0;
                Random r = new Random();

                Product[] products = new Product[3] {await _context.Products.Include(p => p.Category).Where(p => p.Category.Name == "Memory").FirstOrDefaultAsync(),
                                                     await _context.Products.Include(p => p.Category).Where(p => p.Category.Name == "CPU Processors").FirstOrDefaultAsync(),
                                                     await _context.Products.Include(p => p.Category).Where(p => p.Category.Name == "CPU Coolers").FirstOrDefaultAsync()
                };

                Product[] services = new Product[3] { await _context.Products.Where(p => p.Name == "RAM Memory Assembly").FirstOrDefaultAsync(),
                                                      await _context.Products.Where(p => p.Name == "CPU Processor Assembly").FirstOrDefaultAsync(),
                                                      await _context.Products.Where(p => p.Name == "Cooling System Assembly").FirstOrDefaultAsync()
                };

                foreach(var order in appointmentOrders)
                {
                    _context.OrderDetails.Add(new OrderDetail
                    {
                        Order = order,
                        Product = products[increment],                        
                        Quantity = 1,
                        Price = products[increment].Price,
                    });

                    _context.OrderDetails.Add(new OrderDetail
                    {
                        Order = order,
                        Product = services[increment],
                        Quantity = 1,
                        Price = services[increment].Price,
                    });

                    order.TotalQuantity = 2;
                    order.TotalPrice = services[increment].Price + products[increment].Price;

                    increment++;
                }

                var productOrders = await _context.Orders.Where(o => o.Appointment == null).ToListAsync();
                int numberOfProducts;
                var productsList = await _context.Products.Where(p => p.IsService == false).ToListAsync();

                foreach (var order in productOrders)
                {
                    numberOfProducts = r.Next(1, 4);

                    for(int p = 0; p <= numberOfProducts; p++)
                    {
                        var product = productsList[r.Next(productsList.Count)];
                        var quantity = r.Next(1,3);

                        _context.OrderDetails.Add(new OrderDetail
                        {
                            Order = order,
                            Product = product,
                            Quantity = quantity,
                            Price = product.Price
                        });

                        order.TotalPrice += (quantity * product.Price);
                        order.TotalQuantity += quantity;
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task AddStatusesAsync()
        {
            if (!_context.Statuses.Any())
            {
                int nTimes = 6;
                string[] OrderStatusName = new string[6] {
                    "Order Created",
                    "Appointment Created",
                    "Pending Appointment",
                    "Order Shipped",
                    "Appointment Done",
                    "Order Closed"
                };

                string[] OrderStatusDesc = new string[6] {
                    "Pagamento Efetuado S/ marcação",
                    "Pagamento Efetuado C/ marcação",
                    "Pagamento Efetuado C/marcação por efetuar",
                    "",
                    "",
                    ""
                };

                for (int i = 0; i < nTimes; i++)
                {
                    _context.Statuses.Add( new Status
                    {
                        Name = OrderStatusName[i],
                        Description = OrderStatusDesc[i],
                    });
                }
                await _context.SaveChangesAsync();
            }
        }

        private async Task AddOrdersAsync()
        {
            if (!_context.Orders.Any())
            {
                var appointments = await _context.Appointments.ToListAsync();
                var customers = await _userHelper.GetUsersInRoleAsync("Customer");
                var stores = await _context.Stores.Where(s => s.IsOnlineStore == false).ToListAsync();
                
                Random r = new Random();

                //Adiciona as Encomendas das marcações
                foreach(var appointment in appointments)
                {
                    _context.Orders.Add(new Order
                    {
                         Customer = customers[r.Next(customers.Count)],
                         Store = stores[r.Next(stores.Count)],
                         Appointment = appointment,
                         OrderDate = appointment.AppointmentDate.AddDays(r.Next(-10,-1)),
                         DeliveryDate = appointment.AppointmentDate,
                         Status = await _context.Statuses.Where(os => os.Name== "Appointment Created").FirstOrDefaultAsync()
                    });

                }

                //Adiciona encomendas de / para produtos sem marcações

                //TODO comentado por telmo há vezes que dá erro no formato da data, nem sempre - perceber o que se passa com o Filipe

                for (int i = 0; i <= 2; i++)
                {
                    var orderDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, r.Next(DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month)));

                    _context.Orders.Add(new Order
                    {
                        Customer = customers[r.Next(customers.Count)],
                        Store = stores[r.Next(stores.Count)],
                        Appointment = null,
                        OrderDate = orderDate,
                        DeliveryDate = orderDate.AddDays(r.Next(0, 4)),
                        Status= _context.Statuses.Where(s => s.Name == "Order Created").FirstOrDefault()
                    });
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task AddAppointmentsAsync()
        {
            if (!_context.Appointments.Any())
            {
                var technicianUsers = await _userHelper.GetUsersInRoleAsync("Technician");
                Random r = new Random();
                int currentMonth = DateTime.UtcNow.Month;
                int monthTotalDays = DateTime.DaysInMonth(DateTime.UtcNow.Year, currentMonth);
                DateTime startDate = new DateTime(DateTime.UtcNow.Year,currentMonth,1);

                for(int a = 0; a <= 2; a++)
                {
                    var appointmentDate = startDate.AddDays(r.Next(monthTotalDays));
                    var bHour = appointmentDate.AddHours(r.Next(9, 16));
                    var eHour = bHour.AddHours(r.Next(1, 4));
                    var randomBool = r.Next(2) == 1;

                    _context.Appointments.Add(new Appointment
                    {
                        WorkerID = technicianUsers[r.Next(technicianUsers.Count)],
                        AppointmentDate = appointmentDate,
                        BegginingHour = bHour,
                        EndHour = eHour,
                        Comments = "No Comments",
                        HasAttended = randomBool                        
                    });
                }
                await _context.SaveChangesAsync();
            }
        }

        private async Task AddServicesAsync()
        {
            var services = await _context.Products.Where(p => p.IsService == true).ToListAsync();

            if(services == null || services.Count == 0)
            {
                string[] servicesNames = new string[5] { "CPU Processor Assembly", "RAM Memory Assembly", "Cooling System Assembly", "Cable Management Service", "Computer Assembly" };
                string[] servicesDescriptions = new string[5] {
                    "We replace your old processor for the new one bought in our store! Dont worry about the thermal paste no more! We make sure the socket match's your motherboard and that everything runs smoothly!",
                    "Can't you hear the 'clack' of installing the RAM memories? No more worries! Come to us and we will help you analyze the problem without any problem.",
                    "Do you want to install a water cooling system but are afraid of spilling coolant over the motherboard? We can help! Book now to keep that cpu or gpu cool!",
                    "You bought a nice box with acrylics and want the opportunity to see the Leds of your gpu glowing but the cables obscure the brightness of the internal beauty of your computer? No more worries! We will unravel the tangle of cables until you get a wireless pc!",
                    "Don't you know anything about computers? Let the geeks solve the problem!"
                };               
                decimal[] servicesPrices = new decimal[5] { 39.90m, 20m, 39.90m, 24.90m, 99.90m };
                var brand = await _context.Brands.Where(b => b.Name == "WebX").FirstOrDefaultAsync();
                var category = await _context.Categories.Where(c => c.Name == "Services").FirstOrDefaultAsync();


                for (int p = 0; p <= 4; p++)
                {
                    _context.Products.Add(new Product
                    {
                        Brand = brand,
                        Category = category,
                        Name = servicesNames[p],
                        Price = servicesPrices[p],
                        Description = servicesDescriptions[p],
                        IsService = true
                    });
                }

                await _context.SaveChangesAsync();
            }

        }

        private async Task AddStocksAsync()
        {
            if (!_context.Stocks.Any())
            {
                var stores = await _context.Stores.ToListAsync();
                var products = await _context.Products.Where(p => p.IsService == false).ToListAsync();
                int quantity;
                int minimumQuantity = 10;
                Random r = new Random();                

                foreach(var store in stores)
                {
                    foreach(var product in products)
                    {
                        quantity = r.Next(minimumQuantity, 30);

                        _context.Stocks.Add(new Stock
                        {
                            Store = store,
                            Product = product,
                            Quantity = quantity,
                            MinimumQuantity = minimumQuantity
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task AddStoresAsync()
        {
            if (!_context.Stores.Any())
            {
                string[] storesNames = new string[3] { "WebX Online Store", "WebX Lisbon Store", "WebX Porto Store" };
                string[] storesAddresss = new string[3] { "Avenida Nossa Senhora de Fatima, Armazem 23", "Zona Industrial de Alfragide - Estrada Nacional 117", "Rua da Igreja" };
                string[] storesCities = new string[3] { "Leiria", "Amadora", "Maia" };
                string[] storesZipCodes = new string[3] { "2410-140", "2614-520", "4440-452" };
                string storesCountry = "Portugal";
                string[] storesEmails = new string[3] { "webxOnline@gmail.com", "webxLisbon@gmail.com", "webxPorto@gmail.com" };
                string[] storesPhones = new string[3] { "244835530", "213645895", "229448521" };
                bool[] storesAreOnline = new bool[3] { true, false, false };
                bool[] storesAreActive = new bool[3] { true, true, true };

                for (int i = 0; i <= 2; i++)
                {
                    _context.Stores.Add(new Store
                    {
                         Name = storesNames[i],
                         Address = storesAddresss[i],
                         City = storesCities[i],
                         ZipCode = storesZipCodes[i],
                         Country = storesCountry,
                         Email = storesEmails[i],
                         PhoneNumber = storesPhones[i],
                         IsOnlineStore = storesAreOnline[i],
                         IsActive = storesAreActive[i]
                    });
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task AddProductsAsync()
        {
            if (!_context.Products.Any())
            {
                string[] productNames = new string[5] { "Motherboard ATX Asus Prime B550-Plus", "RAM Memory Corsair Vengeance RGB 32GB (2x16GB) DDR5-6000MHz CL36 White", "TX MSI MAG Forge 100M Black Case", "Water Cooler CPU Cooler Master MasterLiquid ML240L V2 RGB 240mm Black", "Intel Core i9-11900K 8-Core 3.5GHz W/Turbo 5.3GHz 16MB Skt1200 Processor"};
                string[] productDescriptions = new string[5] {
                    "The ASUS TUF GAMING B550-PLUS is engineered with tough components, an upgraded power solution and a comprehensive set of cooling options, this motherboard delivers rock-solid performance and unwavering gaming stability for AMD 3rd Gen Ryzen CPUs. ",
                    "CORSAIR VENGEANCE DDR5, optimized for Intel® motherboards, delivers the higher frequencies and greater capacities of DDR5 technology in a high-quality, compact module that suits your system. Tightly-screened high-frequency memory chips power your PC through faster processing, rendering, and buffering than ever, with onboard voltage regulation for easy, finely controlled overclocking. CORSAIR iCUE software enables real-time frequency monitoring, onboard voltage regulation, and Intel XMP 3.0 profile customization. A distinctive solid aluminum heatspreader keeps your memory cool, while ensuring wide compatibility with a huge range of motherboards and CPU coolers. With a limited lifetime warranty as your guarantee of reliability for years to come, VENGEANCE DDR5 continues a long-standing legacy of memory performance.",
                    "Optimised Air Flow - Up to 6 x 120 mm system fans could be installed. Provides the whole system with ventilation for better stability. ARGB Fan Included - Built-in ARGB fan for striking lighting and vivid lighting effect. The 1 to 6 ARGB LED Control board - Bundle with 1 to 6 RGB LED control board that can allow you to have more attractive ways to decorate your gaming rig by the LED strips. Tempered Glass Side panel - Premium-quality 4 mm thick tempered glass design guarantees window durability and viewing capability. Magnetic Filters - The case of magnetic filter on top side is designed to give users the best experience in un-installing and cleaning.",
                    "COOLER MASTER MasterLiquid ML240L RGB V2 is a refreshed design from the popular MasterLiquid Lite Series that elevates the exterior design elements The Pump of the ML240L RGB V2 has been enhanced with the newly designed 3rd Gen Dual Chamber Pump that improves the overall cooling efficiency of the AIO water cooler The radiator surface area has been enlarged to increase the surface area for heat dissipation for optimized cooling performance The ML240L RGB V2 will included the latest SickleFlow 120 RGB fan with a brand new design and Air Balance fan blades that is quieter than before without compromising on air flow and pressure.",
                    "This desktop processor family features an innovative architecture designed for intelligent performance (AI), immersive display and graphics, plus enhanced tuning and expandability to put gamers and PC enthusiasts fully in control of real-world experiences. "
                };
                string[] brandsNames = new string[5] { "Asus", "Corsair", "MSI", "Cooler Master", "Intel" };
                string[] categories = new string[5] { "Motherboards", "Memory", "Cases", "CPU Coolers", "CPU Processors" };
                decimal[] productsPrices = new decimal[5] {153.90m,297.20m,59.90m,71.90m,413.90m};
                Guid[][] images = new Guid[5][] { 
                    new Guid[4] {Guid.Parse("00000000-0000-0000-0000-11000000000a"),Guid.Parse("00000000-0000-0000-0000-11000000000b"),Guid.Parse("00000000-0000-0000-0000-11000000000c"),Guid.Parse("00000000-0000-0000-0000-11000000000e")},
                    new Guid[4] {Guid.Parse("00000000-0000-0000-0000-12000000000a"),Guid.Parse("00000000-0000-0000-0000-12000000000b"),Guid.Parse("00000000-0000-0000-0000-12000000000c"),Guid.Parse("00000000-0000-0000-0000-12000000000d") },
                    new Guid[4] {Guid.Parse("00000000-0000-0000-0000-13000000000a"),Guid.Parse("00000000-0000-0000-0000-13000000000b"),Guid.Parse("00000000-0000-0000-0000-13000000000c"),Guid.Parse("00000000-0000-0000-0000-13000000000d") },
                    new Guid[10] {Guid.Parse("00000000-0000-0000-0000-14000000000a"),Guid.Parse("00000000-0000-0000-0000-14000000000b"),Guid.Parse("00000000-0000-0000-0000-14000000000c"),Guid.Parse("00000000-0000-0000-0000-14000000000d"),Guid.Parse("00000000-0000-0000-0000-14000000000e"),Guid.Parse("00000000-0000-0000-0000-14000000000f"),Guid.Parse("00000000-0000-0000-0000-14000000001a"),Guid.Parse("00000000-0000-0000-0000-14000000001b"),Guid.Parse("00000000-0000-0000-0000-14000000001c"),Guid.Parse("00000000-0000-0000-0000-14000000001d") },
                    new Guid[3] {Guid.Parse("00000000-0000-0000-0000-15000000000a"), Guid.Parse("00000000-0000-0000-0000-15000000000b"), Guid.Parse("00000000-0000-0000-0000-15000000000c")}                
                };

                for(int p = 0; p <= 4; p++)
                {
                    List<ProductImages> productImages = new List<ProductImages>();

                    foreach(var image in images[p])
                    {
                        productImages.Add(new ProductImages
                        {
                            ImageId = image
                        });
                    }

                    _context.Products.Add(new Product
                    {
                        Brand = await _context.Brands.Where(b => b.Name == brandsNames[p]).FirstOrDefaultAsync(),
                        Category = await _context.Categories.Where(c => c.Name == categories[p]).FirstOrDefaultAsync(),
                        Description = productDescriptions[p],
                        Name = productNames[p],
                        Price = productsPrices[p],
                        IsService = false,
                        Images = productImages
                    });
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task AddCategoriesAsync()
        {
            //var categories = new List<Category>();
            if (!_context.Categories.Any())
            {
                int nTimes = 6;
                string[] categ = new string[6] { "Motherboards", "Memory", "Cases", "CPU Coolers", "CPU Processors", "Services" };
                Guid[] images = new Guid[6] { Guid.Parse("00000000-0000-0000-0000-900000000006"), Guid.Parse("00000000-0000-0000-0000-900000000005"), Guid.Parse("00000000-0000-0000-0000-900000000004"), Guid.Parse("00000000-0000-0000-0000-900000000003"), Guid.Parse("00000000-0000-0000-0000-900000000002"), Guid.Parse("00000000-0000-0000-0000-900000000001") };

                for (int i = 0; i < nTimes; i++)
                {
                    var category = new Category
                    {
                        Name = categ[i],
                        ImageId = images[i]
                    };
                    _context.Categories.Add(category);
                }
                await _context.SaveChangesAsync();
            }
        }

        private async Task AddBrandsAsync()
        {
            if (!_context.Brands.Any())
            {
                string[] brandsNames = new string[6] { "Asus","Corsair","MSI","Cooler Master","Intel","WebX"};

                foreach(string brandName in brandsNames)
                {
                    _context.Brands.Add(new Brand
                    {
                        Name = brandName
                    });
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task AddCustomersAsync()
        {
            var users = (List<User>)await _userHelper.GetAllCustomersUsersAsync();

            if(users == null || users.Count == 0)
            {                
                string[] FirstNames = new string[5] {"Micael", "Alexandra", "Constanca", "Simao", "Kyara"};
                string[] LastNames = new string[5] { "Gonçalves", "Brito", "Monteiro", "Gomes", "Vicente" };
                string[] Addresss = new string[5] { "Largo Torres, nº 9, 6º Dir. 5975-651 Albufeira", "Av. St. Matheus Barros, 121, 6º Eq. 3971 São João da Madeira", "Tv. Diogo Fonseca 1481-157 Almada", "Lg. Vicente, 1, 2º Dr. 7789 Leiria", "Rua Denis Leite 4950 Pinhel" };
                string[] Nifs = new string[5] { "256489651", "234897565", "234567826", "231564755", "123597325" };
                string[] phoneNumbers = new string[5] { "253396376", "299722654", "260943096", "224013175", "961385758" };
                Guid[] images = new Guid[5] {Guid.Parse("00000000-0000-0000-0000-100000000001"), Guid.Empty, Guid.Parse("00000000-0000-0000-0000-100000000002"), Guid.Empty, Guid.Parse("00000000-0000-0000-0000-100000000002") };
                int totalCustomers = 4;

                for(int i = 0; i <= totalCustomers; i++)
                {
                    var user = new User
                    {
                        FirstName = FirstNames[i],
                        LastName = LastNames[i],
                        Address = Addresss[i],
                        PhoneNumber = phoneNumbers[i],
                        NIF = Nifs[i],
                        Email = $"{FirstNames[i]}@yopmail.com",
                        UserName = $"{FirstNames[i]}@yopmail.com",
                        ImageId = images[i],
                        Active = true
                    };                    

                    users.Add(user);
                }

                string token = "";

                foreach(var user in users)
                {
                    await _userHelper.AddUserAsync(user, "123456");
                    await _userHelper.AddUserToRoleAsync(user, "Customer");
                    token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                    await _userHelper.ConfirmEmailAsync(user, token);
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task AddProductManagersAsync()
        {
            var user1 = await _userHelper.GetUserByEmailAsync("tatiana@yopmail.com");

            if(user1 == null)
            {
                user1 = new User
                {
                    FirstName = "Tatiana",
                    LastName = "Margins",
                    Address = "R. São. Edgar 6951-087 Setúbal",
                    NIF = "135649527",
                    PhoneNumber = "295049128",
                    Email = "tatiana@yopmail.com",
                    UserName = "tatiana@yopmail.com",
                    ImageId = Guid.Parse("00000000-0000-0000-0000-100000000002"),
                    Active = true
                };

                await _userHelper.AddUserAsync(user1, "123456");

                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user1);
                await _userHelper.ConfirmEmailAsync(user1, token);
            }

            var isInRole1 = await _userHelper.CheckUserInRoleAsync(user1, "Product Manager");

            if (!isInRole1)
            {               
               await _userHelper.AddUserToRoleAsync(user1, "Product Manager");       
            }            

            await _context.SaveChangesAsync();
        }

        private async Task AddTechniciansAsync()
        {
            var user1 = await _userHelper.GetUserByEmailAsync("daniel@yopmail.com");
            var user2 = await _userHelper.GetUserByEmailAsync("ivo@yopmail.com");

            if (user1 == null || user2 == null)
            {
                if(user1 == null)
                {
                    user1 = new User
                    {
                        FirstName = "Daniel",
                        LastName = "Almeida",
                        Address = "Lg. São. Bruno, nº 7 9931 Vila Real de Santo António",
                        NIF = "236548975",
                        PhoneNumber = "252337232",
                        Email = "daniel@yopmail.com",
                        UserName = "daniel@yopmail.com",
                        ImageId = Guid.Empty,
                        Active = true
                    };

                    await _userHelper.AddUserAsync(user1, "123456");

                    var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user1);
                    await _userHelper.ConfirmEmailAsync(user1, token);
                }


                if (user2 == null)
                {
                    user2 = new User
                    {
                        FirstName = "Ivo",
                        LastName = "Lourenço",
                        Address = "Travessa de Correia 8511-536 São Mamede de Infesta",
                        NIF = "236543291",
                        PhoneNumber = "235131161",
                        Email = "ivo@yopmail.com",
                        UserName = "ivo@yopmail.com",
                        ImageId = Guid.Parse("00000000-0000-0000-0000-100000000004"),
                        Active = true
                    };

                    await _userHelper.AddUserAsync(user2, "123456");

                    var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user2);
                    await _userHelper.ConfirmEmailAsync(user2, token);
                }
            }

            var isInRole1 = await _userHelper.CheckUserInRoleAsync(user1, "Technician");
            var isInRole2 = await _userHelper.CheckUserInRoleAsync(user2, "Technician");

            if (!isInRole1 || !isInRole2 )
            {
                if (!isInRole1)
                {
                    await _userHelper.AddUserToRoleAsync(user1, "Technician");
                }

                if (!isInRole2)
                {
                    await _userHelper.AddUserToRoleAsync(user2, "Technician");
                }

            }          

            await _context.SaveChangesAsync();
        }

        private async Task AddAdminsAsync()
        {
            var admins = await _userHelper.GetAllAdminUsersAsync();

            if(admins == null || admins.Count == 0)
            {
                string[] FirstNames = new string[3] { "Filipe", "Telmo", "Stanislav"};
                string[] LastNames = new string[3] { "Ferreira", "Fernandes", "Govera"};
                string[] Addresss = new string[3] { "Avenida Fialho Gouveia N521, Montijo", "Travessa São. Constança, 11, 60º Dir. 4309 Vila Nova de Famalicão", "Av. Lima, 210, 14º Dr. 1011-095 Tomar" };
                string[] Nifs = new string[3] { "275008304", "156321473", "245329758"};
                Guid adminImage = Guid.Parse("00000000-0000-0000-0000-100000000003");
                string[] phoneNumbers = new string[3] { "934565985", "963214569", "963452865"};
                int totalAdmins = 2;

                for (int i = 0; i <= totalAdmins; i++)
                {
                    var user = new User
                    {
                        FirstName = FirstNames[i],
                        LastName = LastNames[i],
                        Address = Addresss[i],
                        PhoneNumber = phoneNumbers[i],
                        NIF = Nifs[i],
                        Email = $"{FirstNames[i]}@yopmail.com",
                        UserName = $"{FirstNames[i]}@yopmail.com",
                        ImageId = adminImage,
                        Active = true
                    };

                    admins.Add(user);
                }

                string token = "";

                foreach (var user in admins)
                {
                    await _userHelper.AddUserAsync(user, "123456");
                    await _userHelper.AddUserToRoleAsync(user, "Admin");
                    token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                    await _userHelper.ConfirmEmailAsync(user, token);
                }

                await _context.SaveChangesAsync();
            }          

        }

        private async Task CheckCreatedRoles()
        {
            await _userHelper.CheckRoleAsync("Admin");
            await _userHelper.CheckRoleAsync("Customer");
            await _userHelper.CheckRoleAsync("Product Manager");
            await _userHelper.CheckRoleAsync("Technician");
        }
    }
}
