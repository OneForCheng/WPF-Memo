using System;
using System.Windows;
using System.Windows.Media.Animation;
using WindowTemplate;
using Message = WindowTemplate.Message;

namespace Notes
{
    public static class Extentions
    {
        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="content"></param>
        public static void ShowMessageBox(string content)
        {
            var msgWin = new MessageWindow(new Message(content, MessageBoxMode.SingleMode))
            {
                //Owner = Owner,
                MiddleBtnContent = "确定(Y)",
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
            msgWin.SetOpacityAnimation(new DoubleAnimation(1, 0.1, new Duration(TimeSpan.FromSeconds(10))), msgWin.Close);
            msgWin.ShowDialog();
        }

        public static void SetPropertyAnimation(this FrameworkElement element, DependencyProperty property, DoubleAnimation animation, Action completedEvent = null)
        {
            //设置动画时段
            Storyboard.SetTargetName(animation, "DynamicByProperty");
            Storyboard.SetTargetProperty(animation, new PropertyPath(property));

            var storyboard = new Storyboard {FillBehavior = FillBehavior.Stop};
            storyboard.Completed += delegate
            {
                completedEvent?.Invoke();
            };
            storyboard.Children.Add(animation);
            element.RegisterName("DynamicByProperty", element);
            storyboard.Begin(element, true);
        }

    }
}
