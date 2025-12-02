using EECOnline.Models.Entities;
using EECOnline.Commons;
using Turbo.DataLayer;
using System.Text;

namespace EECOnline.Models
{
    /// <summary>
    /// 角色功能權限 Model
    /// </summary>
    public class ClamRoleFunc : TblAMFUNCM
    {
        /// <summary>功能資料字串</summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder x = new StringBuilder();
            x.Append("showmenu=");
            x.Append(this.showmenu);
            x.Append(", sysid=");
            x.Append(this.sysid);
            x.Append(", modules=");
            x.Append(this.modules);
            x.Append(", submodules=");
            x.Append(this.submodules);
            x.Append(", prgid=");
            x.Append(this.prgid);
            x.Append(", prgname=");
            x.Append(this.prgname);
            return x.ToString();
        }
    }
}