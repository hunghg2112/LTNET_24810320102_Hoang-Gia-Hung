using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace RetailSalesApp
{
    #region ==================== 1. DTO LAYER (DATA TRANSFER OBJECTS) ====================
    public class NhanVienDTO
    {
        public int MaNV { get; set; }
        public string TenNV { get; set; }
        public string SoDienThoai { get; set; }
        public string ChucVu { get; set; }
    }

    public class KhachHangDTO
    {
        public int MaKH { get; set; }
        public string TenKH { get; set; }
        public string DienThoai { get; set; }
        public int DiemTichLuy { get; set; }
    }

    public class SanPhamDTO
    {
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public decimal GiaBan { get; set; }
        public int SoLuongTon { get; set; }
    }

    public class HoaDonDTO
    {
        public int MaHD { get; set; }
        public DateTime NgayLap { get; set; }
        public int MaNV { get; set; }
        public int? MaKH { get; set; }
        public decimal TongTien { get; set; }
    }

    public class ChiTietHoaDonDTO
    {
        public int MaHD { get; set; }
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }

    public class TopSanPhamDTO
    {
        public string MaSP { get; set; }
        public string TenSP { get; set; }
        public int TongDaBan { get; set; }
    }
    #endregion

    #region ==================== 2. DAL LAYER (DATA ACCESS LAYER) ====================
    public class DatabaseAccess
    {
        protected static string connectionString = @"Server=localhost\SQLEXPRESS;Database=SalesDB;Integrated Security=True;TrustServerCertificate=True;";
    }

    public class SanPhamDAL : DatabaseAccess
    {
        public DataTable GetAll()
        {
            string query = "SELECT MaSP AS [Mã SP], TenSP AS [Tên sản phẩm], GiaBan AS [Giá bán], SoLuongTon AS [Tồn kho] FROM SanPham";
            using (SqlDataAdapter da = new SqlDataAdapter(query, connectionString))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Tìm kiếm sản phẩm 100% bằng SqlParameter với ký tự đại diện %
        public DataTable Search(string keyword)
        {
            string query = @"SELECT MaSP AS [Mã SP], TenSP AS [Tên sản phẩm], GiaBan AS [Giá bán], SoLuongTon AS [Tồn kho] 
                            FROM SanPham 
                            WHERE MaSP LIKE @Keyword OR TenSP LIKE @Keyword";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public bool Insert(SanPhamDTO sp)
        {
            string query = "INSERT INTO SanPham (MaSP, TenSP, GiaBan, SoLuongTon) VALUES (@MaSP, @TenSP, @GiaBan, @SoLuongTon)";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaSP", sp.MaSP);
                    cmd.Parameters.AddWithValue("@TenSP", sp.TenSP);
                    cmd.Parameters.AddWithValue("@GiaBan", sp.GiaBan);
                    cmd.Parameters.AddWithValue("@SoLuongTon", sp.SoLuongTon);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Update(SanPhamDTO sp)
        {
            string query = "UPDATE SanPham SET TenSP = @TenSP, GiaBan = @GiaBan, SoLuongTon = @SoLuongTon WHERE MaSP = @MaSP";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaSP", sp.MaSP);
                    cmd.Parameters.AddWithValue("@TenSP", sp.TenSP);
                    cmd.Parameters.AddWithValue("@GiaBan", sp.GiaBan);
                    cmd.Parameters.AddWithValue("@SoLuongTon", sp.SoLuongTon);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Delete(string maSP)
        {
            string query = "DELETE FROM SanPham WHERE MaSP = @MaSP";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaSP", maSP);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }

    public class KhachHangDAL : DatabaseAccess
    {
        public List<KhachHangDTO> GetAll()
        {
            List<KhachHangDTO> list = new List<KhachHangDTO>();
            string query = "SELECT MaKH, TenKH, DienThoai, DiemTichLuy FROM KhachHang";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new KhachHangDTO
                            {
                                MaKH = (int)reader["MaKH"],
                                TenKH = reader["TenKH"].ToString(),
                                DienThoai = reader["DienThoai"]?.ToString(),
                                DiemTichLuy = Convert.ToInt32(reader["DiemTichLuy"])
                            });
                        }
                    }
                }
            }
            return list;
        }
    }

    public class NhanVienDAL : DatabaseAccess
    {
        public List<NhanVienDTO> GetAll()
        {
            List<NhanVienDTO> list = new List<NhanVienDTO>();
            string query = "SELECT MaNV, TenNV, SoDienThoai, ChucVu FROM NhanVien";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new NhanVienDTO
                            {
                                MaNV = (int)reader["MaNV"],
                                TenNV = reader["TenNV"].ToString(),
                                SoDienThoai = reader["SoDienThoai"]?.ToString(),
                                ChucVu = reader["ChucVu"]?.ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }
    }

    public class HoaDonDAL : DatabaseAccess
    {
        // GIAO DỊCH SQLTRANSACTION: Lập hóa đơn + Chi tiết + Trừ kho + Cộng điểm tích lũy
        public bool CheckoutTransaction(HoaDonDTO hd, List<ChiTietHoaDonDTO> listCT, out string errorMessage)
        {
            errorMessage = "";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    // 1. Chèn bản ghi vào bảng HoaDon
                    string queryHD = @"INSERT INTO HoaDon (NgayLap, MaNV, MaKH, TongTien) 
                                       VALUES (GETDATE(), @MaNV, @MaKH, @TongTien); 
                                       SELECT SCOPE_IDENTITY();";
                    int maHDGenerated = 0;
                    using (SqlCommand cmdHD = new SqlCommand(queryHD, conn, trans))
                    {
                        cmdHD.Parameters.AddWithValue("@MaNV", hd.MaNV);
                        cmdHD.Parameters.AddWithValue("@MaKH", (object)hd.MaKH ?? DBNull.Value);
                        cmdHD.Parameters.AddWithValue("@TongTien", hd.TongTien);
                        maHDGenerated = Convert.ToInt32(cmdHD.ExecuteScalar());
                    }

                    // 2. Chèn từng chi tiết & Trừ SoLuongTon tương ứng trong SanPham
                    foreach (var item in listCT)
                    {
                        // Kiểm tra tồn kho hiện tại
                        string checkStockQuery = "SELECT SoLuongTon FROM SanPham WHERE MaSP = @MaSP";
                        int currentStock = 0;
                        using (SqlCommand cmdCheck = new SqlCommand(checkStockQuery, conn, trans))
                        {
                            cmdCheck.Parameters.AddWithValue("@MaSP", item.MaSP);
                            object res = cmdCheck.ExecuteScalar();
                            currentStock = res != null ? Convert.ToInt32(res) : 0;
                        }

                        if (currentStock < item.SoLuong)
                        {
                            trans.Rollback(); // Rollback nếu tồn kho nhỏ hơn số lượng bán
                            errorMessage = $"Sản phẩm '{item.TenSP}' ({item.MaSP}) hiện chỉ còn {currentStock} trong kho, không đủ bán số lượng {item.SoLuong}!";
                            return false;
                        }

                        // Chèn vào ChiTietHoaDon
                        string queryCT = @"INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia) 
                                          VALUES (@MaHD, @MaSP, @SoLuong, @DonGia)";
                        using (SqlCommand cmdCT = new SqlCommand(queryCT, conn, trans))
                        {
                            cmdCT.Parameters.AddWithValue("@MaHD", maHDGenerated);
                            cmdCT.Parameters.AddWithValue("@MaSP", item.MaSP);
                            cmdCT.Parameters.AddWithValue("@SoLuong", item.SoLuong);
                            cmdCT.Parameters.AddWithValue("@DonGia", item.DonGia);
                            cmdCT.ExecuteNonQuery();
                        }

                        // Trừ tồn kho SanPham
                        string updateStockQuery = "UPDATE SanPham SET SoLuongTon = SoLuongTon - @SoLuong WHERE MaSP = @MaSP";
                        using (SqlCommand cmdStock = new SqlCommand(updateStockQuery, conn, trans))
                        {
                            cmdStock.Parameters.AddWithValue("@SoLuong", item.SoLuong);
                            cmdStock.Parameters.AddWithValue("@MaSP", item.MaSP);
                            cmdStock.ExecuteNonQuery();
                        }
                    }

                    // 3. Cộng điểm tích lũy cho Khách hàng (DiemTichLuy += TongTien / 100000)
                    if (hd.MaKH.HasValue && hd.MaKH.Value > 1) // Bỏ qua Khách lẻ (MaKH = 1)
                    {
                        int addedPoints = (int)(hd.TongTien / 100000);
                        if (addedPoints > 0)
                        {
                            string updatePointsQuery = "UPDATE KhachHang SET DiemTichLuy = DiemTichLuy + @Diem WHERE MaKH = @MaKH";
                            using (SqlCommand cmdPoint = new SqlCommand(updatePointsQuery, conn, trans))
                            {
                                cmdPoint.Parameters.AddWithValue("@Diem", addedPoints);
                                cmdPoint.Parameters.AddWithValue("@MaKH", hd.MaKH.Value);
                                cmdPoint.ExecuteNonQuery();
                            }
                        }
                    }

                    // 4. Commit giao dịch khi thành công
                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    errorMessage = "Lỗi hệ thống: " + ex.Message;
                    return false;
                }
            }
        }

        // Dùng ExecuteScalar tính tổng doanh thu theo khoảng thời gian
        public decimal GetTotalRevenue(DateTime fromDate, DateTime toDate)
        {
            string query = "SELECT ISNULL(SUM(TongTien), 0) FROM HoaDon WHERE NgayLap >= @FromDate AND NgayLap <= @ToDate";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
        }

        // Dùng SqlDataReader đọc danh sách Top 5 sản phẩm bán chạy nhất
        public List<TopSanPhamDTO> GetTop5BestSellers()
        {
            List<TopSanPhamDTO> list = new List<TopSanPhamDTO>();
            string query = @"SELECT TOP 5 sp.MaSP, sp.TenSP, SUM(ct.SoLuong) AS TongDaBan
                             FROM ChiTietHoaDon ct
                             JOIN SanPham sp ON ct.MaSP = sp.MaSP
                             GROUP BY sp.MaSP, sp.TenSP
                             ORDER BY TongDaBan DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new TopSanPhamDTO
                            {
                                MaSP = reader["MaSP"].ToString(),
                                TenSP = reader["TenSP"].ToString(),
                                TongDaBan = Convert.ToInt32(reader["TongDaBan"])
                            });
                        }
                    }
                }
            }
            return list;
        }
    }
    #endregion

    #region ==================== 3. BUS LAYER (BUSINESS LOGIC LAYER) ====================
    public class SalesBUS
    {
        private SanPhamDAL spDAL = new SanPhamDAL();
        private KhachHangDAL khDAL = new KhachHangDAL();
        private NhanVienDAL nvDAL = new NhanVienDAL();
        private HoaDonDAL hdDAL = new HoaDonDAL();

        public DataTable GetSanPhamDataTable() => spDAL.GetAll();
        public DataTable SearchSanPham(string keyword) => spDAL.Search(keyword);
        public List<KhachHangDTO> GetKhachHangList() => khDAL.GetAll();
        public List<NhanVienDTO> GetNhanVienList() => nvDAL.GetAll();

        public bool AddSanPham(SanPhamDTO sp, out string error)
        {
            if (string.IsNullOrWhiteSpace(sp.MaSP) || string.IsNullOrWhiteSpace(sp.TenSP))
            {
                error = "Mã sản phẩm và Tên sản phẩm không được để trống!";
                return false;
            }
            if (sp.GiaBan <= 0 || sp.SoLuongTon < 0)
            {
                error = "Giá bán phải lớn hơn 0 và số lượng tồn không được âm!";
                return false;
            }
            error = "";
            return spDAL.Insert(sp);
        }

        public bool UpdateSanPham(SanPhamDTO sp, out string error)
        {
            if (string.IsNullOrWhiteSpace(sp.TenSP) || sp.GiaBan <= 0 || sp.SoLuongTon < 0)
            {
                error = "Thông tin sản phẩm cập nhật không hợp lệ!";
                return false;
            }
            error = "";
            return spDAL.Update(sp);
        }

        public bool DeleteSanPham(string maSP, out string error)
        {
            try
            {
                error = "";
                return spDAL.Delete(maSP);
            }
            catch (Exception ex)
            {
                error = "Không thể xóa sản phẩm này do đã phát sinh hóa đơn bán hàng!\nChi tiết: " + ex.Message;
                return false;
            }
        }

        public bool Checkout(HoaDonDTO hd, List<ChiTietHoaDonDTO> listCT, out string errorMsg)
        {
            if (listCT == null || listCT.Count == 0)
            {
                errorMsg = "Giỏ hàng hiện tại đang trống!";
                return false;
            }
            return hdDAL.CheckoutTransaction(hd, listCT, out errorMsg);
        }

        public decimal GetRevenueToday()
        {
            DateTime today = DateTime.Today;
            DateTime endOfDay = today.AddDays(1).AddTicks(-1);
            return hdDAL.GetTotalRevenue(today, endOfDay);
        }

        public decimal GetRevenueThisMonth()
        {
            DateTime startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);
            return hdDAL.GetTotalRevenue(startOfMonth, endOfMonth);
        }

        public List<TopSanPhamDTO> GetTop5BestSellers() => hdDAL.GetTop5BestSellers();
    }
    #endregion

    #region ==================== 4. GUI LAYER (GRAPHICAL USER INTERFACE) ====================
    public class frmMainSales : Form
    {
        private SalesBUS bus = new SalesBUS();

        // TAB 1: BÁN HÀNG
        private ComboBox cboNhanVien, cboKhachHang;
        private TextBox txtSearchPOS;
        private DataGridView dgvPOSProducts, dgvCart;
        private NumericUpDown numQuantity;
        private Button btnAddToCart, btnCheckout;
        private Label lblTotalAmount, lblEarnedPoints;
        private List<ChiTietHoaDonDTO> cartList = new List<ChiTietHoaDonDTO>();

        // TAB 2: QUẢN LÝ SẢN PHẨM
        private DataGridView dgvMgmtProducts;
        private TextBox txtMaSP, txtTenSP, txtSearchMgmt;
        private NumericUpDown numGiaBan, numSoLuongTon;
        private Button btnAddSP, btnEditSP, btnDeleteSP;

        // TAB 3: BÁO CÁO THỐNG KÊ
        private Label lblRevenueToday, lblRevenueMonth;
        private DataGridView dgvTop5;
        private Button btnRefreshReport;

        public frmMainSales()
        {
            TaoGiaoDien();
            LoadDataTab1();
            LoadDataTab2();
            LoadDataTab3();
        }

        private void TaoGiaoDien()
        {
            this.Text = "Hệ Thống Quản Lý Bán Hàng & Siêu Thị Mini (Retail Sales System 3-Tier)";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.5F);

            TabControl tabControl = new TabControl { Dock = DockStyle.Fill };
            TabPage tabPOS = new TabPage("Bán hàng & Lập Hóa đơn");
            TabPage tabMgmt = new TabPage("Quản lý Sản phẩm");
            TabPage tabReport = new TabPage("Báo cáo & Thống kê");

            // ---------------- TAB 1: BÁN HÀNG ----------------
            GroupBox gbHeader = new GroupBox { Text = "Thông tin hóa đơn", Location = new Point(15, 10), Size = new Size(1055, 70) };
            gbHeader.Controls.Add(new Label { Text = "Nhân viên:", Location = new Point(15, 30), AutoSize = true });
            cboNhanVien = new ComboBox { Location = new Point(90, 27), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            gbHeader.Controls.Add(cboNhanVien);

            gbHeader.Controls.Add(new Label { Text = "Khách hàng:", Location = new Point(340, 30), AutoSize = true });
            cboKhachHang = new ComboBox { Location = new Point(430, 27), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cboKhachHang.SelectedIndexChanged += (s, e) => CalculateCartTotal();
            gbHeader.Controls.Add(cboKhachHang);

            tabPOS.Controls.Add(gbHeader);

            // Bên trái: Tra cứu & Chọn sản phẩm
            GroupBox gbProducts = new GroupBox { Text = "Danh mục sản phẩm", Location = new Point(15, 90), Size = new Size(480, 520) };
            gbProducts.Controls.Add(new Label { Text = "Tìm SP (Mã/Tên):", Location = new Point(15, 30), AutoSize = true });
            txtSearchPOS = new TextBox { Location = new Point(135, 27), Width = 325 };
            txtSearchPOS.TextChanged += (s, e) => dgvPOSProducts.DataSource = bus.SearchSanPham(txtSearchPOS.Text.Trim());
            gbProducts.Controls.Add(txtSearchPOS);

            dgvPOSProducts = new DataGridView { Location = new Point(15, 70), Size = new Size(445, 380), ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White };
            gbProducts.Controls.Add(dgvPOSProducts);

            gbProducts.Controls.Add(new Label { Text = "SL chọn:", Location = new Point(15, 468), AutoSize = true });
            numQuantity = new NumericUpDown { Location = new Point(80, 465), Width = 70, Minimum = 1, Maximum = 1000, Value = 1 };
            gbProducts.Controls.Add(numQuantity);

            btnAddToCart = new Button { Text = "+ Thêm vào giỏ hàng", Location = new Point(165, 460), Width = 295, Height = 35, BackColor = Color.LightSkyBlue, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            btnAddToCart.Click += BtnAddToCart_Click;
            gbProducts.Controls.Add(btnAddToCart);

            tabPOS.Controls.Add(gbProducts);

            // Bên phải: Giỏ hàng & Thanh toán
            GroupBox gbCart = new GroupBox { Text = "Chi tiết giỏ hàng", Location = new Point(510, 90), Size = new Size(560, 520) };
            dgvCart = new DataGridView { Location = new Point(15, 27), Size = new Size(530, 400), ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White };
            gbCart.Controls.Add(dgvCart);

            lblTotalAmount = new Label { Text = "Tổng tiền: 0 VNĐ", Location = new Point(15, 440), AutoSize = true, Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.DarkRed };
            gbCart.Controls.Add(lblTotalAmount);

            lblEarnedPoints = new Label { Text = "Điểm tích lũy dự kiến: +0 điểm", Location = new Point(15, 475), AutoSize = true, Font = new Font("Segoe UI", 9.5F, FontStyle.Italic), ForeColor = Color.DarkGreen };
            gbCart.Controls.Add(lblEarnedPoints);

            btnCheckout = new Button { Text = "THANH TOÁN (F5)", Location = new Point(340, 445), Width = 205, Height = 55, BackColor = Color.LightGreen, Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
            btnCheckout.Click += BtnCheckout_Click;
            gbCart.Controls.Add(btnCheckout);

            tabPOS.Controls.Add(gbCart);

            // ---------------- TAB 2: QUẢN LÝ SẢN PHẨM ----------------
            GroupBox gbInputSP = new GroupBox { Text = "Thông tin sản phẩm", Location = new Point(15, 15), Size = new Size(350, 595) };
            gbInputSP.Controls.Add(new Label { Text = "Mã sản phẩm:", Location = new Point(15, 40), AutoSize = true });
            txtMaSP = new TextBox { Location = new Point(120, 37), Width = 200 };
            gbInputSP.Controls.Add(txtMaSP);

            gbInputSP.Controls.Add(new Label { Text = "Tên sản phẩm:", Location = new Point(15, 90), AutoSize = true });
            txtTenSP = new TextBox { Location = new Point(120, 87), Width = 200 };
            gbInputSP.Controls.Add(txtTenSP);

            gbInputSP.Controls.Add(new Label { Text = "Giá bán (VNĐ):", Location = new Point(15, 140), AutoSize = true });
            numGiaBan = new NumericUpDown { Location = new Point(120, 137), Width = 200, Maximum = 100000000, Increment = 5000, Value = 10000 };
            gbInputSP.Controls.Add(numGiaBan);

            gbInputSP.Controls.Add(new Label { Text = "Số lượng tồn:", Location = new Point(15, 190), AutoSize = true });
            numSoLuongTon = new NumericUpDown { Location = new Point(120, 187), Width = 200, Maximum = 10000, Value = 10 };
            gbInputSP.Controls.Add(numSoLuongTon);

            btnAddSP = new Button { Text = "Thêm mới", Location = new Point(15, 260), Width = 95, Height = 35, BackColor = Color.LightGreen };
            btnAddSP.Click += BtnAddSP_Click;
            gbInputSP.Controls.Add(btnAddSP);

            btnEditSP = new Button { Text = "Cập nhật", Location = new Point(120, 260), Width = 95, Height = 35, BackColor = Color.Khaki };
            btnEditSP.Click += BtnEditSP_Click;
            gbInputSP.Controls.Add(btnEditSP);

            btnDeleteSP = new Button { Text = "Xóa", Location = new Point(225, 260), Width = 95, Height = 35, BackColor = Color.LightCoral };
            btnDeleteSP.Click += BtnDeleteSP_Click;
            gbInputSP.Controls.Add(btnDeleteSP);

            tabMgmt.Controls.Add(gbInputSP);

            Panel pnlMgmtSearch = new Panel { Location = new Point(380, 15), Size = new Size(690, 45) };
            pnlMgmtSearch.Controls.Add(new Label { Text = "Tìm sản phẩm (SqlParameter %):", Location = new Point(10, 12), AutoSize = true });
            txtSearchMgmt = new TextBox { Location = new Point(220, 9), Width = 450 };
            txtSearchMgmt.TextChanged += (s, e) => dgvMgmtProducts.DataSource = bus.SearchSanPham(txtSearchMgmt.Text.Trim());
            pnlMgmtSearch.Controls.Add(txtSearchMgmt);
            tabMgmt.Controls.Add(pnlMgmtSearch);

            dgvMgmtProducts = new DataGridView { Location = new Point(380, 65), Size = new Size(690, 545), ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White };
            dgvMgmtProducts.SelectionChanged += DgvMgmtProducts_SelectionChanged;
            tabMgmt.Controls.Add(dgvMgmtProducts);

            // ---------------- TAB 3: BÁO CÁO & THỐNG KÊ ----------------
            GroupBox gbStat = new GroupBox { Text = "Doanh thu (Thực thi ExecuteScalar)", Location = new Point(15, 15), Size = new Size(1055, 110) };
            lblRevenueToday = new Label { Text = "Doanh thu hôm nay: 0 VNĐ", Location = new Point(20, 40), AutoSize = true, Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.DarkGreen };
            lblRevenueMonth = new Label { Text = "Doanh thu tháng này: 0 VNĐ", Location = new Point(450, 40), AutoSize = true, Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.DarkBlue };
            gbStat.Controls.Add(lblRevenueToday);
            gbStat.Controls.Add(lblRevenueMonth);

            btnRefreshReport = new Button { Text = "Làm mới số liệu", Location = new Point(880, 35), Width = 150, Height = 40 };
            btnRefreshReport.Click += (s, e) => LoadDataTab3();
            gbStat.Controls.Add(btnRefreshReport);

            tabReport.Controls.Add(gbStat);

            GroupBox gbTop5 = new GroupBox { Text = "Top 5 sản phẩm bán chạy nhất (Thực thi SqlDataReader)", Location = new Point(15, 140), Size = new Size(1055, 470) };
            dgvTop5 = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White };
            gbTop5.Controls.Add(dgvTop5);
            tabReport.Controls.Add(gbTop5);

            tabControl.TabPages.Add(tabPOS);
            tabControl.TabPages.Add(tabMgmt);
            tabControl.TabPages.Add(tabReport);
            this.Controls.Add(tabControl);
        }

        // --- TAB 1 LOGIC ---
        private void LoadDataTab1()
        {
            // Gán Member trước khi gán DataSource
            cboNhanVien.DisplayMember = "TenNV";
            cboNhanVien.ValueMember = "MaNV";
            cboNhanVien.DataSource = bus.GetNhanVienList();

            cboKhachHang.DisplayMember = "TenKH";
            cboKhachHang.ValueMember = "MaKH";
            cboKhachHang.DataSource = bus.GetKhachHangList();

            dgvPOSProducts.DataSource = bus.GetSanPhamDataTable();
            if (dgvPOSProducts.Columns["Giá bán"] != null)
                dgvPOSProducts.Columns["Giá bán"].DefaultCellStyle.Format = "N0";
        }

        private void BtnAddToCart_Click(object sender, EventArgs e)
        {
            if (dgvPOSProducts.SelectedRows.Count == 0) return;

            DataGridViewRow row = dgvPOSProducts.SelectedRows[0];
            string maSP = row.Cells["Mã SP"].Value.ToString();
            string tenSP = row.Cells["Tên sản phẩm"].Value.ToString();
            decimal giaBan = Convert.ToDecimal(row.Cells["Giá bán"].Value);
            int tonKho = Convert.ToInt32(row.Cells["Tồn kho"].Value);
            int qty = (int)numQuantity.Value;

            var existingItem = cartList.Find(x => x.MaSP == maSP);
            int currentCartQty = existingItem != null ? existingItem.SoLuong : 0;

            if (currentCartQty + qty > tonKho)
            {
                MessageBox.Show($"Số lượng yêu cầu vượt quá tồn kho hiện có ({tonKho})!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (existingItem != null)
            {
                existingItem.SoLuong += qty;
            }
            else
            {
                cartList.Add(new ChiTietHoaDonDTO
                {
                    MaSP = maSP,
                    TenSP = tenSP,
                    DonGia = giaBan,
                    SoLuong = qty
                });
            }

            RefreshCartGrid();
        }

        private void RefreshCartGrid()
        {
            dgvCart.DataSource = null;
            dgvCart.DataSource = cartList;
            if (dgvCart.Columns["MaHD"] != null) dgvCart.Columns["MaHD"].Visible = false;
            if (dgvCart.Columns["DonGia"] != null) dgvCart.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            if (dgvCart.Columns["ThanhTien"] != null) dgvCart.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";

            CalculateCartTotal();
        }

        private void CalculateCartTotal()
        {
            decimal total = 0;
            foreach (var item in cartList)
            {
                total += item.ThanhTien;
            }ấ
            lblTotalAmount.Text = $"Tổng tiền: {total:N0} VNĐ";

            
            int? selectedMaKH = null;
            if (cboKhachHang.SelectedItem is KhachHangDTO kh)
            {
                selectedMaKH = kh.MaKH;
            }

            HoaDonDTO hd = new HoaDonDTO
            {
                MaNV = Convert.ToInt32(cboNhanVien.SelectedValue),
                MaKH = selectedMaKH,
                TongTien = 0
            };
            if (selectedMaKH > 1)
            {
                int points = (int)(total / 100000);
                lblEarnedPoints.Text = $"Điểm tích lũy dự kiến: +{points} điểm";
            }
            else
            {
                lblEarnedPoints.Text = "Điểm tích lũy dự kiến: +0 điểm (Khách lẻ)";
            }
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (cboNhanVien.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn Nhân viên lập hóa đơn!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            HoaDonDTO hd = new HoaDonDTO
            {
                MaNV = Convert.ToInt32(cboNhanVien.SelectedValue),
                MaKH = cboKhachHang.SelectedValue != null ? (int?)Convert.ToInt32(cboKhachHang.SelectedValue) : null,
                TongTien = 0
            };
            foreach (var item in cartList) hd.TongTien += item.ThanhTien;

            if (bus.Checkout(hd, cartList, out string errorMsg))
            {
                MessageBox.Show("Thanh toán thành công! Đã tự động trừ tồn kho và cập nhật điểm tích lũy.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                cartList.Clear();
                RefreshCartGrid();
                LoadDataTab1();
                LoadDataTab2();
                LoadDataTab3();
            }
            else
            {
                MessageBox.Show(errorMsg, "Lỗi thanh toán (Rollback)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- TAB 2 LOGIC ---
        private void LoadDataTab2()
        {
            dgvMgmtProducts.DataSource = bus.GetSanPhamDataTable();
            if (dgvMgmtProducts.Columns["Giá bán"] != null) dgvMgmtProducts.Columns["Giá bán"].DefaultCellStyle.Format = "N0";
        }

        private void DgvMgmtProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMgmtProducts.SelectedRows.Count > 0)
            {
                DataGridViewRow r = dgvMgmtProducts.SelectedRows[0];
                txtMaSP.Text = r.Cells["Mã SP"].Value.ToString();
                txtTenSP.Text = r.Cells["Tên sản phẩm"].Value.ToString();
                numGiaBan.Value = Convert.ToDecimal(r.Cells["Giá bán"].Value);
                numSoLuongTon.Value = Convert.ToInt32(r.Cells["Tồn kho"].Value);
                txtMaSP.Enabled = false;
            }
        }

        private void BtnAddSP_Click(object sender, EventArgs e)
        {
            txtMaSP.Enabled = true;
            SanPhamDTO sp = new SanPhamDTO
            {
                MaSP = txtMaSP.Text.Trim(),
                TenSP = txtTenSP.Text.Trim(),
                GiaBan = numGiaBan.Value,
                SoLuongTon = (int)numSoLuongTon.Value
            };

            if (bus.AddSanPham(sp, out string err))
            {
                MessageBox.Show("Thêm sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDataTab1();
                LoadDataTab2();
            }
            else
            {
                MessageBox.Show(err, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnEditSP_Click(object sender, EventArgs e)
        {
            SanPhamDTO sp = new SanPhamDTO
            {
                MaSP = txtMaSP.Text.Trim(),
                TenSP = txtTenSP.Text.Trim(),
                GiaBan = numGiaBan.Value,
                SoLuongTon = (int)numSoLuongTon.Value
            };

            if (bus.UpdateSanPham(sp, out string err))
            {
                MessageBox.Show("Cập nhật sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDataTab1();
                LoadDataTab2();
            }
            else
            {
                MessageBox.Show(err, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnDeleteSP_Click(object sender, EventArgs e)
        {
            string maSP = txtMaSP.Text.Trim();
            if (string.IsNullOrEmpty(maSP)) return;

            if (MessageBox.Show($"Bạn có chắc muốn xóa sản phẩm {maSP}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (bus.DeleteSanPham(maSP, out string err))
                {
                    MessageBox.Show("Xóa sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDataTab1();
                    LoadDataTab2();
                }
                else
                {
                    MessageBox.Show(err, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // --- TAB 3 LOGIC ---
        private void LoadDataTab3()
        {
            decimal todayRev = bus.GetRevenueToday();
            decimal monthRev = bus.GetRevenueThisMonth();

            lblRevenueToday.Text = $"Doanh thu hôm nay: {todayRev:N0} VNĐ";
            lblRevenueMonth.Text = $"Doanh thu tháng này: {monthRev:N0} VNĐ";

            dgvTop5.DataSource = bus.GetTop5BestSellers();
            if (dgvTop5.Columns["MaSP"] != null) dgvTop5.Columns["MaSP"].HeaderText = "Mã SP";
            if (dgvTop5.Columns["TenSP"] != null) dgvTop5.Columns["TenSP"].HeaderText = "Tên sản phẩm";
            if (dgvTop5.Columns["TongDaBan"] != null) dgvTop5.Columns["TongDaBan"].HeaderText = "Tổng số lượng đã bán";
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMainSales());
        }
    }
    #endregion
}