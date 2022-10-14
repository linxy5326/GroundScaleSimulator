namespace xabg.Core
{
    /// 
    /// 选择项类，用于ComboBox或者ListBox添加项
    /// 
    public class ListItem
    {
        private object _vlaue = string.Empty;
        private string _text = string.Empty;
        //可以根据自己的需求继续添加,如：private Int32 m_Index；

        public ListItem()
        { }
        public ListItem(string value, string text)
        {
            _vlaue = value;
            _text = text;
        }
        public override string ToString()
        {
            return this._text;
        }
        public object Value
        {
            get
            {
                return this._vlaue;
            }
            set
            {
                this._vlaue = value;
            }
        }
        public string Text
        {
            get
            {
                return this._text;
            }
            set
            {
                this._text = value;
            }
        }

        private object _tag;

        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }
    }

}
