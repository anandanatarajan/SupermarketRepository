using NPoco;
using SupermarketRepository;

namespace SampleAPIapplication.Models
{
    [TableName("tbl_Qrcode")]
    [PrimaryKey("uid",AutoIncrement =true)]
    public class ClassQrcode 
    {

        public int uid { get; set; }
        public string? qrcode { get; set; }

        public DateTime? created_dt { get; set; }
        public string? rstatus        { get; set; }

        [SuperAutoIncrement]
        public Int32? testfield { get; set; }
        
    }
}
