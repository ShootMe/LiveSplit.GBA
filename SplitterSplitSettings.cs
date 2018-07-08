using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
namespace LiveSplit.GBA {
	public partial class SplitterSplitSettings : UserControl {
		private int mX = 0;
		private int mY = 0;
		private bool isDragging = false;
		public SplitterSplitSettings() {
			InitializeComponent();
			cboSize.SelectedItem = "32 U-Bits";
			cboType.SelectedItem = "Equals";
			cboRAM.SelectedItem = "IWRAM";
			txtOffset.Text = "0";
			txtValue.Text = "0";
			chkSplit.Checked = true;
		}
		private void cboType_Validating(object sender, CancelEventArgs e) {
			string item = GetItemInList(cboSize);
			if (string.IsNullOrEmpty(item)) {
				cboType.SelectedItem = SplitType.Equals;
			} else {
				cboType.SelectedItem = GetEnumValue<SplitType>(item);
			}
		}
		private void cboType_SelectedIndexChanged(object sender, EventArgs e) {
			string splitDescription = cboType.SelectedValue.ToString();
			SplitType split = GetEnumValue<SplitType>(splitDescription);
			if (split == SplitType.Changed) {
				txtValue.Visible = false;
				btnRemove.Location = new System.Drawing.Point(367, 2);
			} else {
				btnRemove.Location = new System.Drawing.Point(441, 2);
				txtValue.Visible = true;
			}
			txtValue.Tag = txtValue.Visible;

			MemberInfo[] infos = typeof(SplitType).GetMember(split.ToString());
			DescriptionAttribute[] descriptions = null;
			if (infos.Length > 0) {
				descriptions = (DescriptionAttribute[])infos[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
			}
			if (descriptions != null && descriptions.Length > 0) {
				ToolTips.SetToolTip(cboType, descriptions[0].Description);
			} else {
				ToolTips.SetToolTip(cboType, string.Empty);
			}
		}
		public static T GetEnumValue<T>(string text) {
			foreach (T item in Enum.GetValues(typeof(T))) {
				string name = item.ToString();

				MemberInfo[] infos = typeof(T).GetMember(name);
				DescriptionAttribute[] descriptions = null;
				if (infos.Length > 0) {
					descriptions = (DescriptionAttribute[])infos[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
				}
				DescriptionAttribute description = descriptions != null && descriptions.Length > 0 ? descriptions[0] : null;

				if (name.Equals(text, StringComparison.OrdinalIgnoreCase) || (description != null && description.Description.Equals(text, StringComparison.OrdinalIgnoreCase))) {
					return item;
				}
			}
			return default(T);
		}
		public static string GetEnumDescription<T>(T item) {
			string name = item.ToString();

			MemberInfo[] infos = typeof(T).GetMember(name);
			DescriptionAttribute[] descriptions = null;
			if (infos.Length > 0) {
				descriptions = (DescriptionAttribute[])infos[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
			}
			DescriptionAttribute description = descriptions != null && descriptions.Length > 0 ? descriptions[0] : null;

			return description == null ? name : description.Description;
		}
		private void cboSize_Validating(object sender, CancelEventArgs e) {
			string item = GetItemInList(cboSize);
			if (string.IsNullOrEmpty(item)) {
				item = "32 U-Bits";
			}
			cboSize.SelectedItem = item;
		}
		private void cboRAM_Validating(object sender, CancelEventArgs e) {
			string item = GetItemInList(cboRAM);
			if (string.IsNullOrEmpty(item)) {
				cboType.SelectedItem = RAMSection.IWRAM;
			} else {
				cboType.SelectedItem = GetEnumValue<RAMSection>(item);
			}
		}
		private void cboSize_SelectedIndexChanged(object sender, EventArgs e) {
			if (cboSize.SelectedItem != null) {
				string sizeDescription = cboSize.SelectedValue.ToString();
				ValueSize size = GetEnumValue<ValueSize>(sizeDescription);
				if (size == GBA.ValueSize.Manual) {
					txtValue.Tag = txtValue.Visible;
					txtValue.Visible = false;
					cboType.Visible = false;
					cboRAM.Visible = false;
					txtOffset.Visible = false;
					chkSplit.Visible = false;
					chkSplit.Checked = true;
					btnRemove.Location = new System.Drawing.Point(127, 2);
				} else {
					bool visible = txtValue.Tag == null ? true : (bool)txtValue.Tag;
					btnRemove.Location = new System.Drawing.Point(visible ? 441 : 367, 2);
					txtValue.Visible = visible;
					cboType.Visible = true;
					cboRAM.Visible = true;
					txtOffset.Visible = true;
					chkSplit.Visible = true;
				}
			}
		}
		private void txtOffset_Validating(object sender, CancelEventArgs e) {
			string offsetText = txtOffset.Text;
			try {
				uint offset = Convert.ToUInt32(offsetText, 16);
				if (offsetText != offsetText.ToUpper()) {
					txtOffset.Text = offsetText.ToUpper();
				}
			} catch {
				txtOffset.Text = "0";
			}
		}
		private void txtValue_Validating(object sender, CancelEventArgs e) {
			long temp;
			if (!long.TryParse(txtValue.Text, NumberStyles.Any, null, out temp) || temp > uint.MaxValue || temp < int.MinValue) {
				txtValue.Text = "0";
			} else {
				txtValue.Text = temp.ToString();
			}
		}
		private string GetItemInList(ComboBox cbo) {
			string item = cbo.Text;
			for (int i = cbo.Items.Count - 1; i >= 0; i += -1) {
				object ob = cbo.Items[i];
				if (ob.ToString().Equals(item, StringComparison.OrdinalIgnoreCase)) {
					return ob.ToString();
				}
			}
			return null;
		}
		private void picHandle_MouseMove(object sender, MouseEventArgs e) {
			if (!isDragging) {
				if (e.Button == MouseButtons.Left) {
					int num1 = mX - e.X;
					int num2 = mY - e.Y;
					if (((num1 * num1) + (num2 * num2)) > 20) {
						DoDragDrop(this, DragDropEffects.All);
						isDragging = true;
						return;
					}
				}
			}
		}
		private void picHandle_MouseDown(object sender, MouseEventArgs e) {
			mX = e.X;
			mY = e.Y;
			isDragging = false;
		}
	}
	public class ToolTipAttribute : Attribute {
		public string ToolTip { get; set; }
		public ToolTipAttribute(string text) {
			ToolTip = text;
		}
	}
}