using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MockPruebaTecnica.Models
{
    public class Producto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_producto")]
        public int IdProducto { get; set; }

        [MaxLength(50)]
        [Column("codigo_barras")]
        public string CodigoBarras { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nombre_producto")]
        public string NombreProducto { get; set; }

        [MaxLength(255)]
        public string Descripcion { get; set; }

        [MaxLength(50)]
        public string Categoria { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Precio { get; set; }

        // Navegación a Ventas
        public ICollection<Venta> Ventas { get; set; }
    }
}
