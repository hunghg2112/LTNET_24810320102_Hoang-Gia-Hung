using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace HRMPayrollApp
{
    #region ==================== 1. DTO LAYER (DATA TRANSFER OBJECTS) ====================
    public class PhongBanDTO
    {
        public int MaPB { get; set; }
        public string TenPB { get; set; }
        public string MoTa { get; set; }
    }

    public class NhanVienDTO
    {
        public int MaNV { get; set; }
        public string HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string CCCD { get; set; }
        public int MaPB { get; set; }
        public string TenPB { get; set; }
        public decimal LuongCoBan { get; set; }
        public double HeSoLuong { get; set; }
    }

    public class BangChamCongDTO
    {
        public int MaCong { get; set; }
        public int MaNV { get; set; }
        public string ThangNam { get; set; }
        public double SoNgayCong { get; set; }
        public double SoGioOT { get; set; }
    }

    public class BangLuongDTO
    {
        public int MaLuong { get; set; }
        public int MaNV { get; set; }
        public string HoTen { get; set; }
        public string TenPB { get; set; }
        public string ThangNam { get; set; }
        public decimal LuongCoBan { get; set; }
        public double HeSoLuong { get; set; }
        public double SoNgayCong { get; set; }
        public double SoGioOT { get; set; }
        public decimal ThucLinh { get; set; }
    }
    #endregion

    #region ==================== 2. DAL LAYER (DATA ACCESS LAYER) ====================
    public class DatabaseAccess
    {
        protected static string connectionString = @"Server=localhost\SQLEXPRESS;Database=HRMDB;Integrated Security=True;TrustServerCertificate=True;";
    }

    public class PhongBanDAL : DatabaseAccess
    {
        public List<PhongBanDTO> GetAll()
        {
            List<PhongBanDTO> list = new List<PhongBanDTO>();
            string query = "SELECT MaPB, TenPB, MoTa FROM PhongBan";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new PhongBanDTO
                            {
                                MaPB = (int)reader["MaPB"],
                                TenPB = reader["TenPB"].ToString(),
                                MoTa = reader["MoTa"]?.ToString()
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
        public DataTable GetAllToDataTable()
        {
            string query = @"SELECT nv.MaNV AS [Mã NV], nv.HoTen AS [Họ và tên], nv.NgaySinh AS [Ngày sinh], 
                            nv.CCCD, pb.TenPB AS [Phòng ban], nv.LuongCoBan AS [Lương CB], nv.HeSoLuong AS [Hệ số], nv.MaPB
                            FROM NhanVien nv
                            JOIN PhongBan pb ON nv.MaPB = pb.MaPB";
            using (SqlDataAdapter da = new SqlDataAdapter(query, connectionString))
            {
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Tìm kiếm nhân viên bằng Parameterized Query
        public DataTable Search(string keyword, int? maPB)
        {
            string query = @"SELECT nv.MaNV AS [Mã NV], nv.HoTen AS [Họ và tên], nv.NgaySinh AS [Ngày sinh], 
                            nv.CCCD, pb.TenPB AS [Phòng ban], nv.LuongCoBan AS [Lương CB], nv.HeSoLuong AS [Hệ số], nv.MaPB
                            FROM NhanVien nv
                            JOIN PhongBan pb ON nv.MaPB = pb.MaPB
                            WHERE (nv.HoTen LIKE @Keyword OR nv.CCCD LIKE @Keyword)";

            if (maPB.HasValue && maPB.Value > 0)
            {
                query += " AND nv.MaPB = @MaPB";
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");
                    if (maPB.HasValue && maPB.Value > 0)
                        cmd.Parameters.AddWithValue("@MaPB", maPB.Value);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public bool Insert(NhanVienDTO nv)
        {
            string query = @"INSERT INTO NhanVien (HoTen, NgaySinh, CCCD, MaPB, LuongCoBan, HeSoLuong) 
                    VALUES (@HoTen, @NgaySinh, @CCCD, @MaPB, @LuongCoBan, @HeSoLuong)";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@HoTen", nv.HoTen);
                    cmd.Parameters.AddWithValue("@NgaySinh", (object)nv.NgaySinh ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CCCD", nv.CCCD);
                    cmd.Parameters.AddWithValue("@MaPB", nv.MaPB);
                    cmd.Parameters.AddWithValue("@LuongCoBan", nv.LuongCoBan);
                    cmd.Parameters.AddWithValue("@HeSoLuong", nv.HeSoLuong);

                    conn.Open();
                    try
                    {
                        return cmd.ExecuteNonQuery() > 0;
                    }
                    catch (SqlException ex)
                    {
                        // Mã lỗi 2627 / 2601 trong SQL Server đại diện cho lỗi trùng khoá UNIQUE
                        if (ex.Number == 2627 || ex.Number == 2601)
                        {
                            throw new Exception($"Số CCCD '{nv.CCCD}' đã tồn tại trong hệ thống! Vui lòng nhập số CCCD khác.");
                        }
                        throw;
                    }
                }
            }
        }

        public bool Update(NhanVienDTO nv)
        {
            string query = @"UPDATE NhanVien 
                            SET HoTen = @HoTen, NgaySinh = @NgaySinh, CCCD = @CCCD, MaPB = @MaPB, 
                                LuongCoBan = @LuongCoBan, HeSoLuong = @HeSoLuong 
                            WHERE MaNV = @MaNV";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaNV", nv.MaNV);
                    cmd.Parameters.AddWithValue("@HoTen", nv.HoTen);
                    cmd.Parameters.AddWithValue("@NgaySinh", (object)nv.NgaySinh ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CCCD", nv.CCCD);
                    cmd.Parameters.AddWithValue("@MaPB", nv.MaPB);
                    cmd.Parameters.AddWithValue("@LuongCoBan", nv.LuongCoBan);
                    cmd.Parameters.AddWithValue("@HeSoLuong", nv.HeSoLuong);

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Delete(int maNV)
        {
            string query = "DELETE FROM NhanVien WHERE MaNV = @MaNV";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MaNV", maNV);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }

    public class ChamCongDAL : DatabaseAccess
    {
        private SqlDataAdapter adapter;
        private SqlCommandBuilder builder;

        // Tự động khởi tạo dữ liệu chấm công mặc định cho nhân viên mới trong tháng
        public void EnsureChamCongExists(string thangNam)
        {
            string initQuery = @"INSERT INTO BangChamCong (MaNV, ThangNam, SoNgayCong, SoGioOT)
                                SELECT nv.MaNV, @ThangNam, 22, 0
                                FROM NhanVien nv
                                WHERE nv.MaNV NOT IN (
                                    SELECT MaNV FROM BangChamCong WHERE ThangNam = @ThangNam
                                )";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(initQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@ThangNam", thangNam);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // NẠP BẢNG CHẤM CÔNG THEO MÔ HÌNH NGẮT KẾT NỐI (DISCONNECTED MODEL)
        public DataTable GetAttendanceDataTable(string thangNam, out SqlDataAdapter outAdapter)
        {
            EnsureChamCongExists(thangNam);

            string query = @"SELECT c.MaCong, c.MaNV, nv.HoTen AS [Họ và tên], pb.TenPB AS [Phòng ban], 
                                   c.ThangNam AS [Tháng/Năm], c.SoNgayCong AS [Số ngày công], c.SoGioOT AS [Số giờ OT]
                            FROM BangChamCong c
                            JOIN NhanVien nv ON c.MaNV = nv.MaNV
                            JOIN PhongBan pb ON nv.MaPB = pb.MaPB
                            WHERE c.ThangNam = @ThangNam";

            SqlConnection conn = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ThangNam", thangNam);

            adapter = new SqlDataAdapter(cmd);

            // Cấu hình các lệnh UPDATE/INSERT/DELETE tự động cho bảng BangChamCong
            string selectBase = "SELECT MaCong, MaNV, ThangNam, SoNgayCong, SoGioOT FROM BangChamCong WHERE ThangNam = '" + thangNam + "'";
            SqlDataAdapter baseAdapter = new SqlDataAdapter(selectBase, connectionString);
            builder = new SqlCommandBuilder(baseAdapter);
            adapter.UpdateCommand = builder.GetUpdateCommand();

            DataTable dt = new DataTable();
            adapter.Fill(dt);

            outAdapter = adapter;
            return dt;
        }

        // ĐỒNG BỘ CẬP NHẬT CHẤM CÔNG BẰNG ADAPTER.UPDATE()
        public bool SaveAttendanceChanges(DataTable dt)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row.RowState == DataRowState.Modified)
                        {
                            string updateSql = @"UPDATE BangChamCong 
                                                SET SoNgayCong = @SoNgayCong, SoGioOT = @SoGioOT 
                                                WHERE MaCong = @MaCong";
                            using (SqlCommand cmd = new SqlCommand(updateSql, conn))
                            {
                                cmd.Parameters.AddWithValue("@SoNgayCong", row["Số ngày công"]);
                                cmd.Parameters.AddWithValue("@SoGioOT", row["Số giờ OT"]);
                                cmd.Parameters.AddWithValue("@MaCong", row["MaCong"]);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    dt.AcceptChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public class BangLuongDAL : DatabaseAccess
    {
        // THỰC THI GIAO DỊCH SQLTRANSACTION TÍNH VÀ CHỐT LƯƠNG THÁNG
        public bool LockPayrollTransaction(string thangNam, List<BangLuongDTO> listLuong, out string errorMessage)
        {
            errorMessage = "";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    // 1. Xóa dữ liệu chốt lương cũ của tháng nếu chốt lại
                    string deleteOld = "DELETE FROM BangLuong WHERE ThangNam = @ThangNam";
                    using (SqlCommand cmdDel = new SqlCommand(deleteOld, conn, trans))
                    {
                        cmdDel.Parameters.AddWithValue("@ThangNam", thangNam);
                        cmdDel.ExecuteNonQuery();
                    }

                    // 2. Chèn từng bản ghi lương chi tiết đã tính toán
                    string insertSql = @"INSERT INTO BangLuong (MaNV, ThangNam, ThucLinh) 
                                        VALUES (@MaNV, @ThangNam, @ThucLinh)";

                    foreach (var item in listLuong)
                    {
                        using (SqlCommand cmdIns = new SqlCommand(insertSql, conn, trans))
                        {
                            cmdIns.Parameters.AddWithValue("@MaNV", item.MaNV);
                            cmdIns.Parameters.AddWithValue("@ThangNam", item.ThangNam);
                            cmdIns.Parameters.AddWithValue("@ThucLinh", item.ThucLinh);
                            cmdIns.ExecuteNonQuery();
                        }
                    }

                    // 3. Commit giao dịch thành công
                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    errorMessage = "Lỗi khi chốt lương: " + ex.Message;
                    return false;
                }
            }
        }

        public List<BangLuongDTO> CalculatePayrollPreview(string thangNam)
        {
            List<BangLuongDTO> list = new List<BangLuongDTO>();
            string query = @"SELECT c.MaNV, nv.HoTen, pb.TenPB, c.ThangNam, nv.LuongCoBan, nv.HeSoLuong, 
                                   c.SoNgayCong, c.SoGioOT
                            FROM BangChamCong c
                            JOIN NhanVien nv ON c.MaNV = nv.MaNV
                            JOIN PhongBan pb ON nv.MaPB = pb.MaPB
                            WHERE c.ThangNam = @ThangNam";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ThangNam", thangNam);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decimal luongCB = Convert.ToDecimal(reader["LuongCoBan"]);
                            double heSo = Convert.ToDouble(reader["HeSoLuong"]);
                            double ngayCong = Convert.ToDouble(reader["SoNgayCong"]);
                            double gioOT = Convert.ToDouble(reader["SoGioOT"]);

                            // Công thức: ThucLinh = (LuongCoBan * HeSoLuong / 22) * SoNgayCong + (SoGioOT * 50,000)
                            decimal thucLinh = (decimal)((double)luongCB * heSo / 22.0 * ngayCong) + (decimal)(gioOT * 50000.0);

                            list.Add(new BangLuongDTO
                            {
                                MaNV = (int)reader["MaNV"],
                                HoTen = reader["HoTen"].ToString(),
                                TenPB = reader["TenPB"].ToString(),
                                ThangNam = reader["ThangNam"].ToString(),
                                LuongCoBan = luongCB,
                                HeSoLuong = heSo,
                                SoNgayCong = ngayCong,
                                SoGioOT = gioOT,
                                ThucLinh = Math.Round(thucLinh, 0)
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
    public class HRMBUS
    {
        private PhongBanDAL pbDAL = new PhongBanDAL();
        private NhanVienDAL nvDAL = new NhanVienDAL();
        private ChamCongDAL ccDAL = new ChamCongDAL();
        private BangLuongDAL luongDAL = new BangLuongDAL();

        public List<PhongBanDTO> GetPhongBanList() => pbDAL.GetAll();
        public DataTable GetNhanVienDataTable() => nvDAL.GetAllToDataTable();
        public DataTable SearchNhanVien(string keyword, int? maPB) => nvDAL.Search(keyword, maPB);

        public bool AddNhanVien(NhanVienDTO nv, out string error)
        {
            if (string.IsNullOrWhiteSpace(nv.HoTen) || string.IsNullOrWhiteSpace(nv.CCCD))
            {
                error = "Họ tên và Số CCCD không được để trống!";
                return false;
            }
            if (nv.LuongCoBan <= 0 || nv.HeSoLuong <= 0)
            {
                error = "Lương cơ bản và hệ số lương phải lớn hơn 0!";
                return false;
            }
            error = "";
            return nvDAL.Insert(nv);
        }

        public bool UpdateNhanVien(NhanVienDTO nv, out string error)
        {
            if (string.IsNullOrWhiteSpace(nv.HoTen) || string.IsNullOrWhiteSpace(nv.CCCD))
            {
                error = "Thông tin nhân viên không hợp lệ!";
                return false;
            }
            error = "";
            return nvDAL.Update(nv);
        }

        public bool DeleteNhanVien(int maNV, out string error)
        {
            try
            {
                error = "";
                return nvDAL.Delete(maNV);
            }
            catch (Exception ex)
            {
                error = "Không thể xóa nhân viên do đã phát sinh chấm công/bảng lương!\nChi tiết: " + ex.Message;
                return false;
            }
        }

        public DataTable GetAttendance(string thangNam, out SqlDataAdapter adapter) => ccDAL.GetAttendanceDataTable(thangNam, out adapter);

        public bool SaveAttendance(DataTable dt) => ccDAL.SaveAttendanceChanges(dt);

        public List<BangLuongDTO> CalculatePayroll(string thangNam) => luongDAL.CalculatePayrollPreview(thangNam);

        public bool LockPayroll(string thangNam, List<BangLuongDTO> listLuong, out string error)
        {
            if (listLuong == null || listLuong.Count == 0)
            {
                error = "Không có dữ liệu chấm công để chốt lương cho tháng này!";
                return false;
            }
            return luongDAL.LockPayrollTransaction(thangNam, listLuong, out error);
        }
    }
    #endregion

    #region ==================== 4. GUI LAYER (GRAPHICAL USER INTERFACE) ====================
    public class frmHRMMain : Form
    {
        private HRMBUS bus = new HRMBUS();

        // TAB 1: NHÂN SỰ
        private DataGridView dgvNhanVien;
        private TextBox txtHoTen, txtCCCD, txtSearch;
        private DateTimePicker dtpNgaySinh;
        private ComboBox cboPhongBan, cboSearchPB;
        private NumericUpDown numLuongCB, numHeSo;
        private Button btnAddNV, btnEditNV, btnDeleteNV;
        private int selectedMaNV = -1;

        // TAB 2: CHẤM CÔNG DISCONNECTED
        private MaskedTextBox mtxtThangNamCC;
        private Button btnLoadCC, btnSaveCC;
        private DataGridView dgvChamCong;
        private DataTable dtChamCong;
        private SqlDataAdapter adapterChamCong;

        // TAB 3: TÍNH LƯƠNG & CHỐT LƯƠNG
        private MaskedTextBox mtxtThangNamLuong;
        private Button btnCalcLuong, btnLockLuong;
        private DataGridView dgvBangLuong;
        private List<BangLuongDTO> currentPayrollList = new List<BangLuongDTO>();

        public frmHRMMain()
        {
            TaoGiaoDien();
            LoadDataTab1();
            LoadDataTab2();
            LoadDataTab3();
        }

        private void TaoGiaoDien()
        {
            this.Text = "Hệ Thống Quản Lý Nhân Sự & Tính Lương (HRM & Payroll System 3-Tier)";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.5F);

            TabControl tabControl = new TabControl { Dock = DockStyle.Fill };
            TabPage tabNV = new TabPage("Hồ sơ Nhân sự");
            TabPage tabCC = new TabPage("Chấm công ngắt kết nối");
            TabPage tabLuong = new TabPage("Tính lương & Chốt lương");

            // ---------------- TAB 1: HỒ SƠ NHÂN SỰ ----------------
            GroupBox gbInput = new GroupBox { Text = "Thông tin nhân viên", Location = new Point(15, 15), Size = new Size(360, 600) };

            gbInput.Controls.Add(new Label { Text = "Họ và tên:", Location = new Point(15, 40), AutoSize = true });
            txtHoTen = new TextBox { Location = new Point(120, 37), Width = 215 };
            gbInput.Controls.Add(txtHoTen);

            gbInput.Controls.Add(new Label { Text = "Ngày sinh:", Location = new Point(15, 85), AutoSize = true });
            dtpNgaySinh = new DateTimePicker { Location = new Point(120, 82), Width = 215, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };
            gbInput.Controls.Add(dtpNgaySinh);

            gbInput.Controls.Add(new Label { Text = "Số CCCD:", Location = new Point(15, 130), AutoSize = true });
            txtCCCD = new TextBox { Location = new Point(120, 127), Width = 215 };
            gbInput.Controls.Add(txtCCCD);

            gbInput.Controls.Add(new Label { Text = "Phòng ban:", Location = new Point(15, 175), AutoSize = true });
            cboPhongBan = new ComboBox { Location = new Point(120, 172), Width = 215, DropDownStyle = ComboBoxStyle.DropDownList };
            gbInput.Controls.Add(cboPhongBan);

            gbInput.Controls.Add(new Label { Text = "Lương CB (VNĐ):", Location = new Point(15, 220), AutoSize = true });
            numLuongCB = new NumericUpDown { Location = new Point(120, 217), Width = 215, Maximum = 500000000, Increment = 500000, Value = 10000000 };
            gbInput.Controls.Add(numLuongCB);

            gbInput.Controls.Add(new Label { Text = "Hệ số lương:", Location = new Point(15, 265), AutoSize = true });
            numHeSo = new NumericUpDown { Location = new Point(120, 262), Width = 215, DecimalPlaces = 2, Increment = 0.1M, Value = 1.0M };
            gbInput.Controls.Add(numHeSo);

            btnAddNV = new Button { Text = "Thêm mới", Location = new Point(15, 330), Width = 100, Height = 38, BackColor = Color.LightGreen };
            btnAddNV.Click += BtnAddNV_Click;
            gbInput.Controls.Add(btnAddNV);

            btnEditNV = new Button { Text = "Cập nhật", Location = new Point(125, 330), Width = 100, Height = 38, BackColor = Color.Khaki };
            btnEditNV.Click += BtnEditNV_Click;
            gbInput.Controls.Add(btnEditNV);

            btnDeleteNV = new Button { Text = "Xóa NV", Location = new Point(235, 330), Width = 100, Height = 38, BackColor = Color.LightCoral };
            btnDeleteNV.Click += BtnDeleteNV_Click;
            gbInput.Controls.Add(btnDeleteNV);

            tabNV.Controls.Add(gbInput);

            Panel pnlSearch = new Panel { Location = new Point(390, 15), Size = new Size(680, 50) };
            pnlSearch.Controls.Add(new Label { Text = "Tìm tên/CCCD:", Location = new Point(10, 15), AutoSize = true });
            txtSearch = new TextBox { Location = new Point(110, 12), Width = 220 };
            txtSearch.TextChanged += (s, e) => PerformSearchNV();
            pnlSearch.Controls.Add(txtSearch);

            pnlSearch.Controls.Add(new Label { Text = "Lọc PB:", Location = new Point(350, 15), AutoSize = true });
            cboSearchPB = new ComboBox { Location = new Point(410, 12), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cboSearchPB.SelectedIndexChanged += (s, e) => PerformSearchNV();
            pnlSearch.Controls.Add(cboSearchPB);

            tabNV.Controls.Add(pnlSearch);

            dgvNhanVien = new DataGridView { Location = new Point(390, 75), Size = new Size(680, 540), ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White };
            dgvNhanVien.SelectionChanged += DgvNhanVien_SelectionChanged;
            tabNV.Controls.Add(dgvNhanVien);

            // ---------------- TAB 2: CHẤM CÔNG DISCONNECTED ----------------
            Panel pnlCCTop = new Panel { Dock = DockStyle.Top, Height = 60 };
            pnlCCTop.Controls.Add(new Label { Text = "Chọn Tháng/Năm (MM/yyyy):", Location = new Point(15, 20), AutoSize = true });
            mtxtThangNamCC = new MaskedTextBox { Location = new Point(200, 17), Width = 100, Mask = "00/0000", Text = "112026" };
            pnlCCTop.Controls.Add(mtxtThangNamCC);

            btnLoadCC = new Button { Text = "Nạp Bảng Chấm Công", Location = new Point(320, 13), Width = 170, Height = 35, BackColor = Color.LightSkyBlue };
            btnLoadCC.Click += BtnLoadCC_Click;
            pnlCCTop.Controls.Add(btnLoadCC);

            btnSaveCC = new Button { Text = "LƯU CHẤM CÔNG (Adapter.Update)", Location = new Point(780, 13), Width = 270, Height = 35, BackColor = Color.LightGreen, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
            btnSaveCC.Click += BtnSaveCC_Click;
            pnlCCTop.Controls.Add(btnSaveCC);

            tabCC.Controls.Add(dgvChamCong);
            dgvChamCong = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White };
            tabCC.Controls.Add(pnlCCTop);
            Panel pnlLuongTop = new Panel { Dock = DockStyle.Top, Height = 65 };
            pnlLuongTop.Controls.Add(new Label { Text = "Chọn Tháng/Năm (MM/yyyy):", Location = new Point(15, 20), AutoSize = true });
            mtxtThangNamLuong = new MaskedTextBox { Location = new Point(200, 17), Width = 100, Mask = "00/0000", Text = "112026" };
            pnlLuongTop.Controls.Add(mtxtThangNamLuong);

            btnCalcLuong = new Button { Text = "Tính & Xem trước Lương", Location = new Point(320, 13), Width = 190, Height = 35, BackColor = Color.Khaki };
            btnCalcLuong.Click += BtnCalcLuong_Click;
            pnlLuongTop.Controls.Add(btnCalcLuong);

            btnLockLuong = new Button { Text = "CHỐT LƯƠNG THÁNG (SqlTransaction)", Location = new Point(740, 13), Width = 310, Height = 38, BackColor = Color.LightGreen, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnLockLuong.Click += BtnLockLuong_Click;
            pnlLuongTop.Controls.Add(btnLockLuong);

            tabLuong.Controls.Add(pnlLuongTop);

            dgvBangLuong = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White };
            tabLuong.Controls.Add(dgvBangLuong);

            tabControl.TabPages.Add(tabNV);
            tabControl.TabPages.Add(tabCC);
            tabControl.TabPages.Add(tabLuong);
            this.Controls.Add(tabControl);
        }

        // --- TAB 1 LOGIC ---
        private void LoadDataTab1()
        {
            var listPB = bus.GetPhongBanList();
            cboPhongBan.DisplayMember = "TenPB"; cboPhongBan.ValueMember = "MaPB";
            cboPhongBan.DataSource = listPB;

            var listSearchPB = new List<PhongBanDTO> { new PhongBanDTO { MaPB = 0, TenPB = "Tất cả phòng ban" } };
            listSearchPB.AddRange(listPB);
            cboSearchPB.DisplayMember = "TenPB"; cboSearchPB.ValueMember = "MaPB";
            cboSearchPB.DataSource = listSearchPB;

            dgvNhanVien.DataSource = bus.GetNhanVienDataTable();
            if (dgvNhanVien.Columns["MaPB"] != null) dgvNhanVien.Columns["MaPB"].Visible = false;
            if (dgvNhanVien.Columns["Lương CB"] != null) dgvNhanVien.Columns["Lương CB"].DefaultCellStyle.Format = "N0";
            if (dgvNhanVien.Columns["Ngày sinh"] != null) dgvNhanVien.Columns["Ngày sinh"].DefaultCellStyle.Format = "dd/MM/yyyy";
        }

        private void PerformSearchNV()
        {
            string kw = txtSearch.Text.Trim();
            int? mapb = cboSearchPB.SelectedValue != null ? (int?)Convert.ToInt32(cboSearchPB.SelectedValue) : null;
            dgvNhanVien.DataSource = bus.SearchNhanVien(kw, mapb);
        }

        private void DgvNhanVien_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvNhanVien.CurrentRow != null && dgvNhanVien.CurrentRow.Index >= 0)
            {
                DataGridViewRow r = dgvNhanVien.CurrentRow;
                if (r.Cells["Mã NV"].Value == null) return;

                selectedMaNV = Convert.ToInt32(r.Cells["Mã NV"].Value);
                txtHoTen.Text = r.Cells["Họ và tên"].Value.ToString();
                txtCCCD.Text = r.Cells["CCCD"].Value.ToString();
                if (r.Cells["Ngày sinh"].Value != DBNull.Value) dtpNgaySinh.Value = Convert.ToDateTime(r.Cells["Ngày sinh"].Value);
                cboPhongBan.SelectedValue = r.Cells["MaPB"].Value;
                numLuongCB.Value = Convert.ToDecimal(r.Cells["Lương CB"].Value);
                numHeSo.Value = Convert.ToDecimal(r.Cells["Hệ số"].Value);
            }
        }

        private void BtnAddNV_Click(object sender, EventArgs e)
        {
            NhanVienDTO nv = new NhanVienDTO
            {
                HoTen = txtHoTen.Text.Trim(),
                NgaySinh = dtpNgaySinh.Value,
                CCCD = txtCCCD.Text.Trim(),
                MaPB = Convert.ToInt32(cboPhongBan.SelectedValue),
                LuongCoBan = numLuongCB.Value,
                HeSoLuong = (double)numHeSo.Value
            };

            try
            {
                if (bus.AddNhanVien(nv, out string err))
                {
                    MessageBox.Show("Thêm nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDataTab1();
                }
                else
                {
                    MessageBox.Show(err, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEditNV_Click(object sender, EventArgs e)
        {
            if (selectedMaNV == -1) return;

            NhanVienDTO nv = new NhanVienDTO
            {
                MaNV = selectedMaNV,
                HoTen = txtHoTen.Text.Trim(),
                NgaySinh = dtpNgaySinh.Value,
                CCCD = txtCCCD.Text.Trim(),
                MaPB = Convert.ToInt32(cboPhongBan.SelectedValue),
                LuongCoBan = numLuongCB.Value,
                HeSoLuong = (double)numHeSo.Value
            };

            if (bus.UpdateNhanVien(nv, out string err))
            {
                MessageBox.Show("Cập nhật thông tin nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDataTab1();
            }
            else
            {
                MessageBox.Show(err, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnDeleteNV_Click(object sender, EventArgs e)
        {
            if (selectedMaNV == -1) return;

            if (MessageBox.Show($"Xác nhận xóa nhân viên Mã {selectedMaNV}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (bus.DeleteNhanVien(selectedMaNV, out string err))
                {
                    MessageBox.Show("Xóa nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDataTab1();
                }
                else
                {
                    MessageBox.Show(err, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // --- TAB 2 LOGIC (DISCONNECTED MODEL) ---
        private void LoadDataTab2()
        {
            BtnLoadCC_Click(null, null);
        }

        private void BtnLoadCC_Click(object sender, EventArgs e)
        {
            string thangNam = mtxtThangNamCC.Text.Trim();
            if (thangNam.Length < 7)
            {
                MessageBox.Show("Vui lòng nhập đúng định dạng Tháng/Năm (MM/yyyy)!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dtChamCong = bus.GetAttendance(thangNam, out adapterChamCong);
            dgvChamCong.DataSource = dtChamCong;

            // Thiết lập chỉ cho sửa Số ngày công & Số giờ OT
            foreach (DataGridViewColumn col in dgvChamCong.Columns)
            {
                if (col.Name == "Số ngày công" || col.Name == "Số giờ OT")
                    col.ReadOnly = false;
                else
                    col.ReadOnly = true;
            }

            if (dgvChamCong.Columns["MaCong"] != null) dgvChamCong.Columns["MaCong"].Visible = false;
            if (dgvChamCong.Columns["MaNV"] != null) dgvChamCong.Columns["MaNV"].Visible = false;
        }

        private void BtnSaveCC_Click(object sender, EventArgs e)
        {
            if (dtChamCong == null) return;

            if (bus.SaveAttendance(dtChamCong))
            {
                MessageBox.Show("Đã đồng bộ cập nhật Bảng chấm công về CSDL thành công (SqlDataAdapter.Update)!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Lỗi khi cập nhật bảng chấm công!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- TAB 3 LOGIC (SQLTRANSACTION PAYROLL) ---
        private void LoadDataTab3()
        {
            BtnCalcLuong_Click(null, null);
        }

        private void BtnCalcLuong_Click(object sender, EventArgs e)
        {
            string thangNam = mtxtThangNamLuong.Text.Trim();
            if (thangNam.Length < 7) return;

            currentPayrollList = bus.CalculatePayroll(thangNam);
            dgvBangLuong.DataSource = null;
            dgvBangLuong.DataSource = currentPayrollList;

            if (dgvBangLuong.Columns["MaLuong"] != null) dgvBangLuong.Columns["MaLuong"].Visible = false;
            if (dgvBangLuong.Columns["MaNV"] != null) dgvBangLuong.Columns["MaNV"].HeaderText = "Mã NV";
            if (dgvBangLuong.Columns["HoTen"] != null) dgvBangLuong.Columns["HoTen"].HeaderText = "Họ và tên";
            if (dgvBangLuong.Columns["TenPB"] != null) dgvBangLuong.Columns["TenPB"].HeaderText = "Phòng ban";
            if (dgvBangLuong.Columns["ThangNam"] != null) dgvBangLuong.Columns["ThangNam"].HeaderText = "Tháng/Năm";
            if (dgvBangLuong.Columns["LuongCoBan"] != null) { dgvBangLuong.Columns["LuongCoBan"].HeaderText = "Lương CB"; dgvBangLuong.Columns["LuongCoBan"].DefaultCellStyle.Format = "N0"; }
            if (dgvBangLuong.Columns["HeSoLuong"] != null) dgvBangLuong.Columns["HeSoLuong"].HeaderText = "Hệ số";
            if (dgvBangLuong.Columns["SoNgayCong"] != null) dgvBangLuong.Columns["SoNgayCong"].HeaderText = "Ngày công";
            if (dgvBangLuong.Columns["SoGioOT"] != null) dgvBangLuong.Columns["SoGioOT"].HeaderText = "Giờ OT";
            if (dgvBangLuong.Columns["ThucLinh"] != null) { dgvBangLuong.Columns["ThucLinh"].HeaderText = "THỰC LĨNH"; dgvBangLuong.Columns["ThucLinh"].DefaultCellStyle.Format = "N0"; dgvBangLuong.Columns["ThucLinh"].DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold); }
        }

        private void BtnLockLuong_Click(object sender, EventArgs e)
        {
            string thangNam = mtxtThangNamLuong.Text.Trim();
            if (currentPayrollList == null || currentPayrollList.Count == 0)
            {
                MessageBox.Show("Vui lòng bấm 'Tính & Xem trước Lương' trước khi chốt!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show($"Xác nhận CHỐT LƯƠNG cho toàn bộ nhân viên tháng {thangNam}?\n(Dữ liệu sẽ được lưu chính thức vào CSDL qua SqlTransaction)",
                                "Xác nhận chốt lương", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (bus.LockPayroll(thangNam, currentPayrollList, out string err))
                {
                    MessageBox.Show($"Chốt lương tháng {thangNam} thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(err, "Lỗi chốt lương (Rollback)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmHRMMain());
        }
    }
    #endregion
}