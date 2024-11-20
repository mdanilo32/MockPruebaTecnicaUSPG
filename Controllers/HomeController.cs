using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockPruebaTecnica.Data;
using MockPruebaTecnica.Models;
using System;
using System.Data;
using System.Diagnostics;

namespace MockPruebaTecnica.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;


        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var totalVentas = await _context.Ventas.SumAsync(v => v.TotalVenta);
            var totalUnidades = await _context.Ventas.SumAsync(v => v.Cantidad);

            var ventasPorMarca = _context.Ventas
                .Include(v => v.Producto)
                .GroupBy(v => v.Producto.Categoria)
                .Select(g => new
                {
                    Marca = g.Key,
                    TotalVentas = g.Sum(v => v.TotalVenta)
                })
                .ToList();

            var ventasPorMes = _context.Ventas
                .GroupBy(v => new { v.FechaVenta.Year, v.FechaVenta.Month })
                .Select(g => new
                {
                    Mes = g.Key.Month,
                    Año = g.Key.Year,
                    TotalVentas = g.Sum(v => v.TotalVenta)
                })
                .OrderBy(g => g.Año).ThenBy(g => g.Mes)
                .ToList();

            var topProductos = _context.Ventas
                .GroupBy(v => v.IdProducto)
                .Select(g => new
                {
                    Producto = g.FirstOrDefault().Producto.NombreProducto,
                    CantidadVendida = g.Sum(v => v.Cantidad)
                })
                .OrderByDescending(g => g.CantidadVendida)
                .Take(10)
                .ToList();

            ViewData["TotalVentas"] = totalVentas;
            ViewData["TotalUnidades"] = totalUnidades;
            ViewData["VentasPorMes"] = ventasPorMes;
            ViewData["VentasPorMarca"] = ventasPorMarca;
            ViewData["TopProductos"] = topProductos;
            return View();
        }

        public IActionResult CargarArchivo()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost()]
        public async Task<IActionResult> Upload(IFormFile salesFile)
        {
            if (salesFile == null || salesFile.Length == 0)
            {
                ViewData["Message"] = "Please upload a valid Excel (.xlsx) file.";
                return View("UploadSales");
            }

            var salesData = new List<Venta>();
            var clientesData = new List<Cliente>();

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = new MemoryStream())
            {
                await salesFile.CopyToAsync(stream);
                stream.Position = 0;

                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true
                        }
                    });

                    var table = dataSet.Tables[0];

                    foreach (DataRow row in table.Rows)
                    {
                        var cliente = new Cliente
                        {
                            Nombre = row.Field<string>("nombre"),
                            Apellido = row.Field<string>("apellido"),
                            CorreoElectronico = row.Field<string>("correo_electronico"),

                        };

                        _context.Clientes.Add(cliente);
                        await _context.SaveChangesAsync();


                        var producto = new Producto
                        {
                            Categoria = row.Field<string>("categoria"),
                            Precio = Convert.ToDecimal(row.Field<double>("precio")),
                            CodigoBarras = row.Field<string>("codigo_barras"),
                            NombreProducto = row.Field<string>("nombre_producto"),
                            Descripcion = row.Field<string>("descripcion"),

                        };

                        _context.Productos.Add(producto);
                        await _context.SaveChangesAsync();

                        var venta = new Venta
                        {
                            FechaVenta = row.Field<DateTime>("fecha_venta"),
                            IdCliente = cliente.IdCliente,
                            IdProducto = producto.IdProducto,
                            Cantidad = Convert.ToInt32(row.Field<double>("cantidad")),
                            TotalVenta = Convert.ToDecimal(row.Field<double>("total_venta"))
                        };

                        salesData.Add(venta);
                    }
                }
            }

            // Save all sales data to the database
            _context.Ventas.AddRange(salesData);
            await _context.SaveChangesAsync();

            ViewData["Message"] = "Sales data successfully uploaded and processed!";
            return View("CargarArchivo");
        }
    }
}
