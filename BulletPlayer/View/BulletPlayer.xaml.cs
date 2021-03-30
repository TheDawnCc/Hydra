using System.Windows.Controls;
using BulletPlayer.Units.BulletScreen.ViewModel;

namespace BulletPlayer.View
{
    /// <summary>
    /// BulletPlayer.xaml 的交互逻辑
    /// </summary>
    public partial class BulletPlayer : UserControl
    {
        public BulletPlayer()
        {
            InitializeComponent();

            var test = new BulletScreenViewModel("675014");
        }
    }
}
