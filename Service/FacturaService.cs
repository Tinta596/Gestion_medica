using Gestion.Interface;
using Gestion.Model;
using Gestion.Data.Repositories;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GestionMedica.Services
{
    // ─────────────────────────────────────────────────────────────
    // FontResolver: lee fuentes TTF directamente de Windows
    // ─────────────────────────────────────────────────────────────
    public class WindowsFontResolver : IFontResolver
    {
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            string lower = familyName.ToLowerInvariant();

            if (lower == "verdana")
            {
                if (isBold && isItalic) return new FontResolverInfo("Verdana#BI");
                if (isBold) return new FontResolverInfo("Verdana#B");
                if (isItalic) return new FontResolverInfo("Verdana#I");
                return new FontResolverInfo("Verdana#R");
            }

            if (isBold) return new FontResolverInfo("Arial#B");
            return new FontResolverInfo("Arial#R");
        }

        public byte[] GetFont(string faceName)
        {
            string fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

            string file;
            if (faceName == "Verdana#R") file = "verdana.ttf";
            else if (faceName == "Verdana#B") file = "verdanab.ttf";
            else if (faceName == "Verdana#I") file = "verdanai.ttf";
            else if (faceName == "Verdana#BI") file = "verdanaz.ttf";
            else if (faceName == "Arial#B") file = "arialbd.ttf";
            else file = "arial.ttf";

            string fullPath = Path.Combine(fontsFolder, file);
            if (!File.Exists(fullPath))
                fullPath = Path.Combine(fontsFolder, "arial.ttf");

            return File.ReadAllBytes(fullPath);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // FacturaServices
    // ─────────────────────────────────────────────────────────────
    public class FacturaServices : IFactura
    {
        private readonly FacturaRepository _repository;

        static FacturaServices()
        {
            if (GlobalFontSettings.FontResolver == null)
                GlobalFontSettings.FontResolver = new WindowsFontResolver();
        }

        public FacturaServices()
        {
            _repository = new FacturaRepository();
        }


        private async Task<Usuario> ObtenerUsuario(int idUsuario)
        {
            try
            {
                return await _repository.ObtenerUsuarioAsync(idUsuario);
            }
            catch
            {
                return null;
            }
        }

        private async Task<Citas> ObtenerCitaPorFactura(int idFactura)
        {
            try
            {
                return await _repository.ObtenerCitaPorFacturaAsync(idFactura);
            }
            catch
            {
                return null;
            }
        }

        private void DrawCampo(XGraphics gfx, XFont font, XFont fontBold,
                               string label, string valor, double x, double y)
        {
            gfx.DrawString(label, fontBold, XBrushes.Black, x, y);
            gfx.DrawString(valor ?? "-", font,
                new XSolidBrush(XColor.FromArgb(60, 60, 60)), x + 110, y);
        }

        // ── ObtenerFactura ───────────────────────────────────────
        public async Task<Factura> ObtenerFactura(int id)
        {
            try
            {
                var factura = await _repository.ObtenerFacturaAsync(id);

                if (factura.Total == 0m && factura.Items.Count > 0)
                    factura.Total = factura.Items.Sum(i => i.Subtotal);

                return factura;
            }
            catch
            {
                return new Factura { Items = new List<FacturaItem>() };
            }
        }

        // ── ObtenerFacturasPorUsuario ────────────────────────────
        public async Task<List<Factura>> ObtenerFacturasPorUsuario(int idUsuario)
        {
            try
            {
                return await _repository.ObtenerFacturasUsuarioAsync(idUsuario);
            }
            catch
            {
                return new List<Factura>();
            }
        }

        // ── CrearFactura ─────────────────────────────────────────
        public async Task<int> CrearFactura(Factura factura)
        {
            try
            {
                return await _repository.CrearFacturaAsync(factura);
            }
            catch
            {
                return 0;
            }
        }

        // ── GenerarPdfFactura ────────────────────────────────────
        public async Task<bool> GenerarPdfFactura(int id, string path)
        {
            try
            {
                var factura = await ObtenerFactura(id);
                if (factura == null || factura.Id == 0) return false;

                var usuario = await ObtenerUsuario(factura.IdUsuario);
                var cita = await ObtenerCitaPorFactura(factura.Id);

                // Fuentes
                var font = new XFont("Verdana", 9, XFontStyleEx.Regular);
                var fontBold = new XFont("Verdana", 9, XFontStyleEx.Bold);
                var fontTitle = new XFont("Verdana", 15, XFontStyleEx.Bold);
                var fontLabel = new XFont("Verdana", 8, XFontStyleEx.Bold);

                // Colores
                var azulOscuro = new XSolidBrush(XColor.FromArgb(30, 80, 140));
                var azulClaro = new XSolidBrush(XColor.FromArgb(180, 210, 240));
                var gris = new XSolidBrush(XColor.FromArgb(90, 90, 90));
                var grisClaro = new XSolidBrush(XColor.FromArgb(245, 245, 245));
                var lineaPen = new XPen(XColor.FromArgb(30, 80, 140), 0.5);

                var doc = new PdfDocument();
                doc.Info.Title = $"Factura #{factura.Id}";
                var page = doc.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                double W = page.Width;

                // ══ BANDA SUPERIOR ══════════════════════════════
                gfx.DrawRectangle(azulOscuro, 0, 0, W, 72);

                gfx.DrawString("GESTIÓN MÉDICA", fontTitle, XBrushes.White,
                    new XRect(30, 12, W / 2, 30), XStringFormats.TopLeft);
                gfx.DrawString("Sistema de Gestión de Citas y Facturación",
                    new XFont("Verdana", 8, XFontStyleEx.Regular),
                    azulClaro, new XRect(30, 40, W / 2, 18), XStringFormats.TopLeft);

                gfx.DrawString($"FACTURA  #{factura.Id:D4}",
                    new XFont("Verdana", 13, XFontStyleEx.Bold),
                    XBrushes.White, new XRect(0, 16, W - 30, 24), XStringFormats.TopRight);
                gfx.DrawString($"Estado: {factura.Estado}",
                    new XFont("Verdana", 8, XFontStyleEx.Regular),
                    azulClaro, new XRect(0, 44, W - 30, 16), XStringFormats.TopRight);

                double y = 90;

                // ══ DATOS DEL PACIENTE ═══════════════════════════
                gfx.DrawString("DATOS DEL PACIENTE", fontLabel, azulOscuro, 30, y);
                y += 13;
                gfx.DrawLine(lineaPen, 30, y, W - 30, y);
                y += 10;

                if (usuario != null)
                {
                    DrawCampo(gfx, font, fontBold, "Paciente:", $"{usuario.Nombre} {usuario.Apellido}", 30, y); y += 16;
                    DrawCampo(gfx, font, fontBold, "Correo:", usuario.Correo, 30, y); y += 16;
                    DrawCampo(gfx, font, fontBold, "Teléfono:", usuario.Telefono, 30, y); y += 16;
                    DrawCampo(gfx, font, fontBold, "Dirección:", usuario.Direccion, 30, y); y += 16;
                }
                else
                {
                    gfx.DrawString($"ID Usuario: {factura.IdUsuario}", font, gris, 30, y); y += 16;
                }

                y += 12;

                // ══ DATOS DE LA CITA ══════════════════════════════
                gfx.DrawString("DATOS DE LA CITA", fontLabel, azulOscuro, 30, y);
                y += 13;
                gfx.DrawLine(lineaPen, 30, y, W - 30, y);
                y += 10;

                if (cita != null)
                {
                    DrawCampo(gfx, font, fontBold, "Cita N°:", cita.Id.ToString(), 30, y); y += 16;
                    DrawCampo(gfx, font, fontBold, "Fecha y hora:", cita.FechaHora.ToString("dd/MM/yyyy HH:mm"), 30, y); y += 16;
                    DrawCampo(gfx, font, fontBold, "Especialidad:", cita.IdEspecialidad.ToString(), 30, y); y += 16;
                    DrawCampo(gfx, font, fontBold, "Estado cita:", cita.Estado.ToString(), 30, y); y += 16;

                    if (!string.IsNullOrWhiteSpace(cita.Motivo))
                    {
                        DrawCampo(gfx, font, fontBold, "Motivo:", cita.Motivo, 30, y); y += 16;
                    }
                    if (!string.IsNullOrWhiteSpace(cita.Observaciones))
                    {
                        DrawCampo(gfx, font, fontBold, "Observaciones:", cita.Observaciones, 30, y); y += 16;
                    }
                }
                else
                {
                    gfx.DrawString("No se encontró información de la cita.", font, gris, 30, y); y += 16;
                }

                y += 14;

                // ══ DETALLE DE SERVICIOS ══════════════════════════
                gfx.DrawString("DETALLE DE SERVICIOS", fontLabel, azulOscuro, 30, y);
                y += 13;

                // Cabecera tabla
                gfx.DrawRectangle(azulOscuro, 30, y, W - 60, 20);
                gfx.DrawString("Descripción", fontBold, XBrushes.White, 36, y + 5);
                gfx.DrawString("Precio", fontBold, XBrushes.White, W - 230, y + 5);
                gfx.DrawString("Cant.", fontBold, XBrushes.White, W - 160, y + 5);
                gfx.DrawString("Subtotal", fontBold, XBrushes.White, W - 90, y + 5);
                y += 20;

                bool filaPar = false;
                if (factura.Items != null && factura.Items.Count > 0)
                {
                    foreach (var it in factura.Items)
                    {
                        if (filaPar) gfx.DrawRectangle(grisClaro, 30, y, W - 60, 18);

                        gfx.DrawString(it.Descripcion, font, XBrushes.Black, 36, y + 4);
                        gfx.DrawString(it.Precio.ToString("N2"), font, XBrushes.Black, W - 230, y + 4);
                        gfx.DrawString(it.Cantidad.ToString(), font, XBrushes.Black, W - 160, y + 4);
                        gfx.DrawString(it.Subtotal.ToString("N2"), font, XBrushes.Black, W - 90, y + 4);
                        y += 18;
                        filaPar = !filaPar;

                        if (y > page.Height - 100)
                        {
                            page = doc.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            y = 40;
                        }
                    }
                }
                else
                {
                    gfx.DrawString("Sin ítems registrados.", font, gris, 36, y + 4);
                    y += 18;
                }

                // Línea separadora
                y += 8;
                gfx.DrawLine(new XPen(XColor.FromArgb(30, 80, 140), 1), 30, y, W - 30, y);
                y += 10;

                // Total
                gfx.DrawRectangle(azulOscuro, W - 205, y, 175, 28);
                gfx.DrawString("TOTAL:",
                    new XFont("Verdana", 10, XFontStyleEx.Bold),
                    XBrushes.White, W - 200, y + 8);
                gfx.DrawString($"$ {factura.Total:N2}",
                    new XFont("Verdana", 10, XFontStyleEx.Bold),
                    XBrushes.White, W - 130, y + 8);

                // Pie de página
                double yFoot = page.Height - 28;
                gfx.DrawLine(new XPen(XColor.FromArgb(200, 200, 200), 0.5), 30, yFoot, W - 30, yFoot);
                gfx.DrawString(
                    $"Generado el {factura.Fecha:dd/MM/yyyy HH:mm}  —  Gestión Médica",
                    new XFont("Verdana", 7, XFontStyleEx.Regular),
                    gris, new XRect(30, yFoot + 5, W - 60, 14), XStringFormats.TopLeft);

                // Guardar
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                doc.Save(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ── CancelarFactura ──────────────────────────────────────
        public async Task<bool> CancelarFactura(int id)
        {
            try
            {
                return await _repository.CancelarFacturaAsync(id);
            }
            catch
            {
                return false;
            }
        }
    }
}
