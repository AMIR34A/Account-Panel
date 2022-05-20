namespace CliBotWinForm
{
    public partial class AccountPanelForm : Form
    {
        AutoResetEvent resetEvent;
        WTelegram.Client client;
        Classes.Process process;
        string filePath;
        List<long> selectedUserId = new List<long>();
        public AccountPanelForm()
        {
            InitializeComponent();
        }

        private async void AccountPanelForm_Load(object sender, EventArgs e)
        {
            var sessionFilePath = Path.Combine(Application.StartupPath.Remove(Application.StartupPath.Length - 22, 22), "WTelegram.session");
            if (!File.Exists(sessionFilePath))
                ChangeEnabledOfGroupBoxAndLabel(false);
            else
            {
                PhoneNumberTextBox.Text = Properties.Settings.Default.PhoneNumber;
                ApiIdTextBox.Text = Properties.Settings.Default.ApiId;
                ApiHashTextBox.Text = Properties.Settings.Default.ApiHash;
                ApiIdAndHashTextbox_MouseLeave(this, new EventArgs());
                await ChangeSituationOfLogin();
                await FillChats();
            }
        }


        private void LogInButton_Click(object sender, EventArgs e)
        {
            resetEvent.Set();
            LogInButton.Visible = PasswordTextBox.Visible = PasswordLabel.Visible = false;
        }


        private async void SendCodeButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ApiHash = ApiHashTextBox.Text;
            Properties.Settings.Default.ApiId = ApiIdTextBox.Text;
            Properties.Settings.Default.PhoneNumber = PhoneNumberTextBox.Text;
            Properties.Settings.Default.Save();
            LogInButton.Visible = PasswordTextBox.Visible = PasswordLabel.Visible = true;
            await ChangeSituationOfLogin();
        }

        private async Task ChangeSituationOfLogin()
        {
            SendCodeButton.Enabled = false;
            client = new WTelegram.Client(Config);
            client.CollectAccessHash = true;
            var user = await client.LoginUserIfNeeded();
            StatusLabel.ForeColor = Color.Green;
            StatusLabel.Text = $"{user.first_name} {user.last_name}(Online)";
            ChangeEnabledOfGroupBoxAndLabel(true);
        }

        private async Task FillChats()
        {
            process = new Classes.Process(client);
            var chats = await process.GetAllChatsAsync();
            foreach (var chat in chats)
            {
                if (chat.TypeChat == Classes.TypeChat.Channel)
                    ChannelsCheckedListBox.Items.Add($"{chat.Title} :{chat.Id}");
                else if (chat.TypeChat == Classes.TypeChat.Group)
                    GroupsCheckedListBox.Items.Add($"{chat.Title} :{chat.Id}");
                else
                    PrivateChatsCheckedListBox.Items.Add($"{chat.Title} :{chat.Id}");
            }
        }

        private void ChangeEnabledOfGroupBoxAndLabel(bool status)
        {
            AccountInformationGroupBox.Enabled = true;
            PhoneNumberTextBox.ReadOnly = ApiIdTextBox.ReadOnly = ApiHashTextBox.ReadOnly = CodeTextBox.ReadOnly = status;
            PrivateChatsGroupBox.Enabled = status;
            GroupsGroupBox.Enabled = status;
            ChannelsGroupBox.Enabled = status;
            SendMessageGroupBox.Enabled = status;
        }

        public string Config(string what)
        {
            switch (what)
            {
                case "api_id": return Properties.Settings.Default.ApiId;
                case "api_hash": return Properties.Settings.Default.ApiHash;
                case "phone_number": return Properties.Settings.Default.PhoneNumber;
                case "verification_code":
                    resetEvent = new AutoResetEvent(false);
                    var signal = resetEvent.WaitOne();
                    return CodeTextBox.Text;
                case "password":
                    return PasswordTextBox.Text;
                default: return null;                  // let WTelegramClient decide the default config
            }
        }

        private void ApiIdAndHashTextbox_MouseLeave(object sender, EventArgs eventArgs)
        {
            ApiIdTextBox.PasswordChar = ApiHashTextBox.PasswordChar = '*';
        }
        private void ApiIdAndHashTextbox_MouseEnter(object sender, EventArgs eventArgs)
        {
            ApiIdTextBox.PasswordChar = ApiHashTextBox.PasswordChar = '\U00000000';
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private async void SelectMediaButton_Click(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Title = "Choose a file";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = fileDialog.FileName;
            }
        }

        private async void SendButton_Click(object sender, EventArgs e)
        {
            if (selectedUserId.Count == 0)
                return;
  
            await process.SendMessageAsync(filePath, MessageTextBox.Text, selectedUserId);
            filePath = "";
            selectedUserId.Clear();
            PrivateChatsCheckedListBox.SelectedItems.Clear();
            GroupsCheckedListBox.SelectedItems.Clear();
            ChannelsCheckedListBox.SelectedItems.Clear();
        }

        private void CheckedListBox_SeleteItem(object sender, ItemCheckEventArgs e)
        {
            var userId = long.Parse(((Control)sender).Text.Split(':')[1]);

            if (e.NewValue == CheckState.Checked)
                selectedUserId.Add(userId);
            else
                selectedUserId.Remove(userId);
        }
    }
}