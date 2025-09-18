using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pedidos.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "Cliente"),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FechaPedido = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            // Insertar datos iniciales
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Nombre", "Password", "Rol" },
                values: new object[,]
                {
                    {
                        1,
                        "admin@pedidos.com",
                        "Administrador",
                        "$2a$11$r3x4X7fL9Yq1w2z3c4v5B.6n7m8o9p0qR1s2t3u4v5w6x7y8z9A0B",
                        "Admin"
                    },
                    {
                        2,
                        "empleado@pedidos.com",
                        "Empleado Ejemplo",
                        "$2a$11$r3x4X7fL9Yq1w2z3c4v5B.6n7m8o9p0qR1s2t3u4v5w6x7y8z9A0B",
                        "Empleado"
                    },
                    {
                        3,
                        "cliente@pedidos.com",
                        "Cliente Ejemplo",
                        "$2a$11$r3x4X7fL9Yq1w2z3c4v5B.6n7m8o9p0qR1s2t3u4v5w6x7y8z9A0B",
                        "Cliente"
                    }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Categoria", "Descripcion", "Nombre", "Precio", "Stock" },
                values: new object[,]
                {
                    {
                        1,
                        "Electrónicos",
                        "Último modelo de smartphone con 128GB de almacenamiento",
                        "Smartphone X5",
                        599.99m,
                        50
                    },
                    {
                        2,
                        "Audio",
                        "Auriculares inalámbricos con cancelación de ruido",
                        "Auriculares Bluetooth",
                        89.99m,
                        100
                    },
                    {
                        3,
                        "Electrónicos",
                        "Tablet de 10 pulgadas con 64GB de almacenamiento",
                        "Tablet Pro 10\"",
                        329.99m,
                        30
                    },
                    {
                        4,
                        "Wearables",
                        "Reloj inteligente con monitor de actividad física",
                        "Smartwatch Series 4",
                        199.99m,
                        25
                    },
                    {
                        5,
                        "Accesorios",
                        "Cargador rápido inalámbrico compatible con Qi",
                        "Cargador Inalámbrico",
                        39.99m,
                        200
                    }
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "Estado", "Notas", "Total", "UserId" },
                values: new object[,]
                {
                    { 1, "Entregado", "Entregar en recepción", 689.98m, 3 },
                    { 2, "Enviado", "LLamar antes de entregar", 199.99m, 3 }
                });

            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "Id", "Cantidad", "OrderId", "PrecioUnitario", "ProductId", "Subtotal" },
                values: new object[,]
                {
                    { 1, 1, 1, 599.99m, 1, 599.99m },
                    { 2, 1, 1, 89.99m, 2, 89.99m },
                    { 3, 1, 2, 199.99m, 4, 199.99m }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}