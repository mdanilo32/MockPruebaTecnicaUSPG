using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MockPruebaTecnica.Models
{
    public class Venta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_venta")]
        public int IdVenta { get; set; }

        [Required]
        [Column("fecha_venta")]
        public DateTime FechaVenta { get; set; }

        [Required]
        [Column("id_cliente")]
        public int IdCliente { get; set; }

        [Required]
        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        [Column("total_venta", TypeName = "decimal(10, 2)")]
        public decimal TotalVenta { get; set; }

        [ForeignKey("IdCliente")]
        public Cliente Cliente { get; set; }

        [ForeignKey("IdProducto")]
        public Producto Producto { get; set; }
    }
}
