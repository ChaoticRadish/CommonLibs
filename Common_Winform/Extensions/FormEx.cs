﻿using Common_Winform.Controls.FeatureGroup;
using Common_Winform.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common_Winform.Extensions
{
    public static class FormEx
    {
        /// <summary>
        /// 阻塞窗口, 会在UI线程中修改阻塞状态
        /// </summary>
        /// <param name="b"></param>
        /// <param name="actionText">阻塞是为了执行什么事情, 如果不是空字符串, 将同时调用<see cref="ShowWaitingForm(Form, string)"/>显示等待窗口</param>
        public static void Obstruction(this Form form, bool b, string? actionText = null)
        {
            form.AutoInvoke(new Action(() =>
            {
                form.Enabled = !b;
                form.UseWaitCursor = b;
                if (b)
                {
                    if (!string.IsNullOrEmpty(actionText))
                    {
                        ShowWaitingForm(form, actionText);
                    }
                }
                else
                {
                    HideWaitingForm(form);
                }

            }));
        }

        /// <summary>
        /// 展示等待窗口
        /// </summary>
        /// <param name="form"></param>
        /// <param name="showingText"></param>
        public static void ShowWaitingForm(this Form form, string showingText = "请稍等...")
        {
            form.AutoInvoke(() =>
            {
                bool showed = false;
                foreach (Control c in form.Controls)
                {
                    if (c is WaitingInfoBox box)
                    {
                        box.Info = showingText;
                        box.Show();
                        showed = true;
                    }
                }
                if (showed) return;
                form.Controls.Add(new WaitingInfoBox()
                {
                    Info = showingText,
                    CirculCount = 12,
                });
            });
        }
        /// <summary>
        /// 隐藏等待窗口
        /// </summary>
        /// <param name="form"></param>
        public static void HideWaitingForm(this Form form)
        {
            form.AutoInvoke(() =>
            {
                foreach (Control c in form.Controls)
                {
                    if (c is WaitingInfoBox)
                    {
                        form.Controls.Remove(c);
                        c.Dispose();
                    }
                }
            });
        }
        /// <summary>
        /// 设置等待窗口的文本信息
        /// </summary>
        /// <param name="form"></param>
        /// <param name="showingText"></param>
        public static void SetWaitingFormText(this Form form, string showingText = "请稍等...")
        {
            form.AutoInvoke(() =>
            {
                foreach (Control c in form.Controls)
                {
                    if (c is WaitingInfoBox box)
                    {
                        box.Info = showingText;
                    }
                }
            });
        }

        /// <summary>
        /// 弹窗询问是否OK
        /// </summary>
        /// <param name="form"></param>
        /// <param name="showingText"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static bool AskOk(this Form form, string showingText, string title)
        {
            //return DialogResult.OK 
            //    == MessageBox.Show(
            //        form,
            //        showingText, title, 
            //        MessageBoxButtons.OKCancel, 
            //        MessageBoxIcon.Question);

            return DialogResult.OK
                == new AskOkCancelForm01()
                {
                    Title = title,
                    ShowingText = showingText,
                    StartPosition = FormStartPosition.CenterParent,
                }.ShowDialog(form);
        }
    }
}
