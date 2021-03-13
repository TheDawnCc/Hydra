using BulletPlayer.Units.BulletScreen.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletPlayer.Kits;
using BulletPlayer.Units.BulletScreen.Model.Bili;

namespace BulletPlayer.Units.BulletScreen.ViewModel
{
    public class BulletScreenViewModel
    {
        private Queue<BulletModel> bulletQueue;


        public BulletScreenViewModel(string roomId)
        {
            var test = new BiliBulletWebSock(roomId);
        }
    }
}
