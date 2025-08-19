using APP_CHECKOUT.Utilities.Lib;
using System;

/// <summary>
/// Class quản lý các công thức giá cho hệ thống Besmal (Đã tối ưu hóa)
/// </summary>
public class BesmalPriceFormulaManager
{
    /// <summary>
    /// Công thức 1: Tính giá nhập
    /// Đầu vào: gia_niem_yet (Giá niêm yết), ty_le_sncc (Tỷ lệ SNcc %), ty_le_chiet_khau (% chiết khấu)
    /// Đầu ra: Giá nhập (G)
    /// Công thức: G = Y - (Y * SNcc) - (Y * X)
    /// </summary>
    public decimal tinh_gia_nhap(decimal gia_niem_yet, decimal ty_le_sncc, decimal ty_le_chiet_khau)
    {
        return gia_niem_yet - (gia_niem_yet * ty_le_sncc) - (gia_niem_yet * ty_le_chiet_khau);
    }

    /// <summary>
    /// Công thức 2: Tính giá bán cuối khi có sale
    /// Đầu vào: gia_niem_yet (Giá niêm yết), ty_le_sale_ncc (% Sale NCC mà Best Mall để sale)
    /// Đầu ra: Giá bán cuối GC(Y_Sale)
    /// Công thức: GC(Y_Sale) = Y - (Y × BS)
    /// </summary>
    public decimal tinh_gia_ban_cuoi_co_sale(decimal gia_niem_yet, decimal ty_le_sale_ncc)
    {
        return gia_niem_yet - (gia_niem_yet * ty_le_sale_ncc);
    }

    /// <summary>
    /// Công thức 3: Tính tổng giá bán theo số lượng khi chưa sale (Đã tối ưu)
    /// Đầu vào: gia_niem_yet (Giá niêm yết), so_luong (Số lượng sản phẩm)
    /// Đầu ra: Tổng giá bán GM(Y)
    /// Công thức: GM(Y) = Y * SL (vì GC(Y) = Y)
    /// </summary>
    public decimal tinh_tong_gia_ban_chua_sale(decimal gia_niem_yet, int so_luong)
    {
        return gia_niem_yet * so_luong;
    }

    /// <summary>
    /// Công thức 4: Tính tổng giá bán theo số lượng khi có sale (Đã tối ưu)
    /// Đầu vào: gia_niem_yet (Giá niêm yết), ty_le_sale_ncc (% Sale NCC), so_luong (Số lượng sản phẩm)
    /// Đầu ra: Tổng giá bán GM(Y_Sale)
    /// Công thức: GM(Y_Sale) = (Y - Y × BS) * SL
    /// </summary>
    public decimal tinh_tong_gia_ban_co_sale(decimal gia_niem_yet, decimal ty_le_sale_ncc, int so_luong)
    {
        decimal gia_ban_cuoi_sale = tinh_gia_ban_cuoi_co_sale(gia_niem_yet, ty_le_sale_ncc);
        return gia_ban_cuoi_sale * so_luong;
    }

    /// <summary>
    /// Công thức 5: Tính lợi nhuận tạm tính khi chưa sale
    /// Đầu vào: gia_niem_yet (Giá niêm yết), ty_le_chiet_khau (% chiết khấu), ty_le_sncc (Tỷ lệ SNcc %), so_luong (Số lượng)
    /// Đầu ra: Lợi nhuận tạm tính LTT(Y)
    /// Công thức: LTT(Y) = Y * (X + SNcc) * SL
    /// </summary>
    public decimal tinh_loi_nhuan_tam_tinh_chua_sale(decimal gia_niem_yet, decimal ty_le_chiet_khau, decimal ty_le_sncc, int so_luong)
    {
        return gia_niem_yet * (ty_le_chiet_khau + ty_le_sncc) * so_luong;
    }

    /// <summary>
    /// Công thức 6: Tính lợi nhuận tạm tính sau sale
    /// Đầu vào: gia_niem_yet (Giá niêm yết), ty_le_chiet_khau (% chiết khấu), ty_le_sncc (Tỷ lệ SNcc %), ty_le_sale_ncc (% Sale NCC), so_luong (Số lượng)
    /// Đầu ra: Lợi nhuận tạm tính LTT(Y_Sale)
    /// Công thức: Nếu 0 <= BS <= SNcc → Y * (X + (SNcc - BS)) * SL, Nếu không → Y * (X + 0) * SL
    /// </summary>
    public decimal tinh_loi_nhuan_tam_tinh_sau_sale(decimal gia_niem_yet, decimal ty_le_chiet_khau, decimal ty_le_sncc, decimal ty_le_sale_ncc, int so_luong)
    {
        if (ty_le_sale_ncc >= 0 && ty_le_sale_ncc <= ty_le_sncc)
        {
            return gia_niem_yet * (ty_le_chiet_khau + (ty_le_sncc - ty_le_sale_ncc)) * so_luong;
        }
        else
        {
            return gia_niem_yet * (ty_le_chiet_khau + 0) * so_luong;
        }
    }

    /// <summary>
    /// Công thức 7: Tính % Sale của best mall
    /// Đầu vào: ty_le_sale_ncc (% Sale NCC), ty_le_sncc (Tỷ lệ SNcc %)
    /// Đầu ra: % Sale của best mall
    /// Công thức: BS - SNcc (nếu BS > SNcc, ngược lại = 0)
    /// </summary>
    public decimal tinh_phan_tram_sale_best_mall(decimal ty_le_sale_ncc, decimal ty_le_sncc)
    {
        if (ty_le_sale_ncc > ty_le_sncc)
        {
            return ty_le_sale_ncc - ty_le_sncc;
        }
        return 0;
    }

    /// <summary>
    /// Kiểm tra điều kiện áp dụng voucher vận chuyển
    /// Đầu vào: tong_gia_ban (Tổng giá bán), dieu_kien_voucher_ship (Điều kiện áp voucher ship)
    /// Đầu ra: true nếu đủ điều kiện, false nếu không
    /// </summary>
    private bool kiem_tra_dieu_kien_voucher_ship(decimal tong_gia_ban, decimal dieu_kien_voucher_ship)
    {
        return tong_gia_ban >= dieu_kien_voucher_ship;
    }

    /// <summary>
    /// Kiểm tra điều kiện áp dụng voucher giảm giá
    /// Đầu vào: tong_gia_ban (Tổng giá bán), dieu_kien_voucher_giam_gia (Điều kiện áp voucher giảm giá)
    /// Đầu ra: true nếu đủ điều kiện, false nếu không
    /// </summary>
    private bool kiem_tra_dieu_kien_voucher_giam_gia(decimal tong_gia_ban, decimal dieu_kien_voucher_giam_gia)
    {
        return tong_gia_ban >= dieu_kien_voucher_giam_gia;
    }

    /// <summary>
    /// Công thức 8: Tính giá khách hàng phải trả khi chưa sale (Đã tối ưu)
    /// Đầu vào: gia_niem_yet (Giá niêm yết), so_luong (Số lượng), phi_van_chuyen (Phí vận chuyển), 
    ///          voucher_van_chuyen (Voucher vận chuyển), ty_le_voucher_giam_gia (% voucher giảm giá), 
    ///          dieu_kien_voucher_ship (Điều kiện áp voucher ship), dieu_kien_voucher_giam_gia (Điều kiện áp voucher giảm giá)
    /// Đầu ra: Giá khách hàng thanh toán GKTT(Y)
    /// Công thức: GKTT(Y) = GM(Y) + VC - VVC - (GM(Y) * VGG)
    /// </summary>
    public decimal tinh_gia_khach_hang_tra_chua_sale(decimal gia_niem_yet, int so_luong, decimal phi_van_chuyen, 
        decimal voucher_van_chuyen, decimal ty_le_voucher_giam_gia, decimal dieu_kien_voucher_ship, decimal dieu_kien_voucher_giam_gia)
    {
        decimal tong_gia_ban = tinh_tong_gia_ban_chua_sale(gia_niem_yet, so_luong);
        
        decimal voucher_van_chuyen_ap_dung = kiem_tra_dieu_kien_voucher_ship(tong_gia_ban, dieu_kien_voucher_ship) ? voucher_van_chuyen : 0;
        decimal ty_le_voucher_giam_gia_ap_dung = kiem_tra_dieu_kien_voucher_giam_gia(tong_gia_ban, dieu_kien_voucher_giam_gia) ? ty_le_voucher_giam_gia : 0;
        
        return tong_gia_ban + phi_van_chuyen - voucher_van_chuyen_ap_dung - (tong_gia_ban * ty_le_voucher_giam_gia_ap_dung);
    }

    /// <summary>
    /// Công thức 9: Tính giá khách hàng phải trả khi có sale (Đã tối ưu)
    /// Đầu vào: gia_niem_yet (Giá niêm yết), ty_le_sale_ncc (% Sale NCC), so_luong (Số lượng), 
    ///          phi_van_chuyen (Phí vận chuyển), voucher_van_chuyen (Voucher vận chuyển), 
    ///          ty_le_voucher_giam_gia (% voucher giảm giá), dieu_kien_voucher_ship (Điều kiện áp voucher ship), 
    ///          dieu_kien_voucher_giam_gia (Điều kiện áp voucher giảm giá)
    /// Đầu ra: Giá khách hàng thanh toán GKTT(Y_Sale)
    /// Công thức: GKTT(Y_Sale) = GM(Y_Sale) + VC - VVC - (GM(Y_Sale) * VGG)
    /// </summary>
    public decimal tinh_gia_khach_hang_tra_co_sale(decimal gia_niem_yet, decimal ty_le_sale_ncc, int so_luong, 
        decimal phi_van_chuyen, decimal voucher_van_chuyen, decimal ty_le_voucher_giam_gia, 
        decimal dieu_kien_voucher_ship, decimal dieu_kien_voucher_giam_gia)
    {
        decimal tong_gia_ban_sale = tinh_tong_gia_ban_co_sale(gia_niem_yet, ty_le_sale_ncc, so_luong);
        decimal tong_gia_ban_goc = tinh_tong_gia_ban_chua_sale(gia_niem_yet, so_luong); // Dùng GM(Y) để kiểm tra điều kiện DKG
        
        decimal voucher_van_chuyen_ap_dung = kiem_tra_dieu_kien_voucher_ship(tong_gia_ban_sale, dieu_kien_voucher_ship) ? voucher_van_chuyen : 0;
        decimal ty_le_voucher_giam_gia_ap_dung = kiem_tra_dieu_kien_voucher_giam_gia(tong_gia_ban_goc, dieu_kien_voucher_giam_gia) ? ty_le_voucher_giam_gia : 0;
        
        return tong_gia_ban_sale + phi_van_chuyen - voucher_van_chuyen_ap_dung - (tong_gia_ban_sale * ty_le_voucher_giam_gia_ap_dung);
    }

    /// <summary>
    /// Công thức 10: Tính lợi nhuận ròng khi chưa sale (Đã tối ưu)
    /// Đầu vào: gia_niem_yet (Giá niêm yết), ty_le_chiet_khau (% chiết khấu), ty_le_sncc (Tỷ lệ SNcc %), 
    ///          so_luong (Số lượng), phi_affiliate (Phí affiliate), phi_vnpay (Phí VN Pay), 
    ///          phi_van_chuyen (Phí vận chuyển), voucher_van_chuyen (Voucher vận chuyển), 
    ///          ty_le_voucher_giam_gia (% voucher giảm giá), dieu_kien_voucher_ship (Điều kiện áp voucher ship), 
    ///          dieu_kien_voucher_giam_gia (Điều kiện áp voucher giảm giá)
    /// Đầu ra: Lợi nhuận ròng LR
    /// Công thức: LR = LTT(Y) - (GKTT(Y) * A) - (GKTT(Y) * P) - VVC - (GM(Y) * VGG)
    /// </summary>
    public decimal tinh_loi_nhuan_rong_chua_sale(decimal gia_niem_yet, decimal ty_le_chiet_khau, decimal ty_le_sncc, 
        int so_luong, decimal phi_affiliate, decimal phi_vnpay, decimal phi_van_chuyen, decimal voucher_van_chuyen, 
        decimal ty_le_voucher_giam_gia, decimal dieu_kien_voucher_ship, decimal dieu_kien_voucher_giam_gia)
    {
        decimal loi_nhuan_tam_tinh = tinh_loi_nhuan_tam_tinh_chua_sale(gia_niem_yet, ty_le_chiet_khau, ty_le_sncc, so_luong);
        decimal gia_khach_hang_thanh_toan = tinh_gia_khach_hang_tra_chua_sale(gia_niem_yet, so_luong, phi_van_chuyen, voucher_van_chuyen, ty_le_voucher_giam_gia, dieu_kien_voucher_ship, dieu_kien_voucher_giam_gia);
        decimal tong_gia_ban = tinh_tong_gia_ban_chua_sale(gia_niem_yet, so_luong);
        
        decimal voucher_van_chuyen_ap_dung = kiem_tra_dieu_kien_voucher_ship(tong_gia_ban, dieu_kien_voucher_ship) ? voucher_van_chuyen : 0;
        decimal ty_le_voucher_giam_gia_ap_dung = kiem_tra_dieu_kien_voucher_giam_gia(tong_gia_ban, dieu_kien_voucher_giam_gia) ? ty_le_voucher_giam_gia : 0;
        
        return loi_nhuan_tam_tinh - (gia_khach_hang_thanh_toan * phi_affiliate) - (gia_khach_hang_thanh_toan * phi_vnpay) - voucher_van_chuyen_ap_dung - (tong_gia_ban * ty_le_voucher_giam_gia_ap_dung);
    }

    /// <summary>
    /// Công thức 11: Tính lợi nhuận ròng sau sale (Đã tối ưu)
    /// Đầu vào: gia_niem_yet (Giá niêm yết), ty_le_chiet_khau (% chiết khấu), ty_le_sncc (Tỷ lệ SNcc %), 
    ///          ty_le_sale_ncc (% Sale NCC), so_luong (Số lượng), phi_affiliate (Phí affiliate), 
    ///          phi_vnpay (Phí VN Pay), phi_van_chuyen (Phí vận chuyển), voucher_van_chuyen (Voucher vận chuyển), 
    ///          ty_le_voucher_giam_gia (% voucher giảm giá), dieu_kien_voucher_ship (Điều kiện áp voucher ship), 
    ///          dieu_kien_voucher_giam_gia (Điều kiện áp voucher giảm giá)
    /// Đầu ra: Lợi nhuận ròng LR
    /// Công thức: LR = LTT(Y_Sale) - (GKTT(Y_Sale) * A) - (GKTT(Y_Sale) * P) - VVC - (GM(Y_Sale) * VGG) - (Y * SBM)
    /// Lưu ý: SBM được tính từ công thức 7: tinh_phan_tram_sale_best_mall(ty_le_sale_ncc, ty_le_sncc)
    /// </summary>
    public decimal tinh_loi_nhuan_rong_sau_sale(decimal gia_niem_yet, decimal ty_le_chiet_khau, decimal ty_le_sncc, 
        decimal ty_le_sale_ncc, int so_luong, decimal phi_affiliate, decimal phi_vnpay, decimal phi_van_chuyen, 
        decimal voucher_van_chuyen, decimal ty_le_voucher_giam_gia, decimal dieu_kien_voucher_ship, decimal dieu_kien_voucher_giam_gia)
    {
        decimal loi_nhuan_tam_tinh_sale = tinh_loi_nhuan_tam_tinh_sau_sale(gia_niem_yet, ty_le_chiet_khau, ty_le_sncc, ty_le_sale_ncc, so_luong);
        decimal gia_khach_hang_thanh_toan_sale = tinh_gia_khach_hang_tra_co_sale(gia_niem_yet, ty_le_sale_ncc, so_luong, phi_van_chuyen, voucher_van_chuyen, ty_le_voucher_giam_gia, dieu_kien_voucher_ship, dieu_kien_voucher_giam_gia);
       

        decimal tong_gia_ban_sale = tinh_tong_gia_ban_co_sale(gia_niem_yet, ty_le_sale_ncc, so_luong);
        decimal tong_gia_ban_goc = tinh_tong_gia_ban_chua_sale(gia_niem_yet, so_luong);
        
        // Tính SBM từ công thức 7
        decimal phan_tram_sale_best_mall = tinh_phan_tram_sale_best_mall(ty_le_sale_ncc, ty_le_sncc);
        
        decimal voucher_van_chuyen_ap_dung = kiem_tra_dieu_kien_voucher_ship(tong_gia_ban_sale, dieu_kien_voucher_ship) ? voucher_van_chuyen : 0;
        decimal ty_le_voucher_giam_gia_ap_dung = kiem_tra_dieu_kien_voucher_giam_gia(tong_gia_ban_goc, dieu_kien_voucher_giam_gia) ? ty_le_voucher_giam_gia : 0;
        LogHelper.InsertLogTelegram(@"[APP.CHECKOUT] MainServices -tinh_loi_nhuan_rong_sau_sale: 
            " + loi_nhuan_tam_tinh_sale + @"
            , " + gia_khach_hang_thanh_toan_sale + @"
             , " + tong_gia_ban_sale + @"
             , " + tong_gia_ban_goc + @"
             , " + phan_tram_sale_best_mall + @"
             , " + voucher_van_chuyen_ap_dung + @"
             , " + ty_le_voucher_giam_gia_ap_dung + @"

            ;");
        return loi_nhuan_tam_tinh_sale - (gia_khach_hang_thanh_toan_sale * phi_affiliate) - (gia_khach_hang_thanh_toan_sale * phi_vnpay) - (phi_van_chuyen * voucher_van_chuyen_ap_dung) - (tong_gia_ban_sale * ty_le_voucher_giam_gia_ap_dung) - (gia_niem_yet * phan_tram_sale_best_mall);
    }
}