using System.Collections.Generic;

namespace BulletPlayer.Units.BulletScreen.Model.Bili
{
    public class GetTokenDataModel
    {
        public string group;
        public int business_id;
        public float refresh_row_factor;
        public int refresh_rate;
        public int max_delay;
        public string token;
        public List<GetTokenDataHostListModel> host_list;
    }
}
