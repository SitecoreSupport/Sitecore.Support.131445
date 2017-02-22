using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell;
using Sitecore.Shell.Applications.ContentManager.Galleries;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XmlControls;
using System.Linq;

namespace Sitecore.Support.Shell.Applications.ContentManager.Galleries.Languages
{
    
    /// <summary>
    /// Represents a gallery languages form.
    /// </summary>
    public class GalleryLanguagesForm : GalleryForm
    {
        /// <summary></summary>
        protected GalleryMenu Options;
        /// <summary></summary>
        protected Scrollbox Languages;
        /// <summary>
        /// Handles the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            if (message.Name == "event:click")
            {
                return;
            }
            base.Invoke(message, true);
        }
        /// <summary>
        /// Raises the load event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        /// <remarks>
        /// This method notifies the server control that it should perform actions common to each HTTP
        /// request for the page it is associated with, such as setting up a database query. At this
        /// stage in the page lifecycle, server controls in the hierarchy are created and initialized,
        /// view state is restored, and form controls reflect client-side data. Use the IsPostBack
        /// property to determine whether the page is being loaded in response to a client postback,
        /// or if it is being loaded and accessed for the first time.
        /// </remarks>
        protected override void OnLoad(System.EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnLoad(e);
            if (Context.ClientPage.IsEvent)
            {
                return;
            }
            Item currentItem = GalleryLanguagesForm.GetCurrentItem();
            if (currentItem == null)
            {
                return;
            }
            using (new ThreadCultureSwitcher(Context.Language.CultureInfo))
            {
                System.Collections.Generic.IEnumerable<Language> enumerable = currentItem.Languages;
                if (Settings.ContentEditor.SortLanguages)
                {
                    enumerable = enumerable.OrderBy((Language x) => x, new LanguageComparer(currentItem.Database));
                }
                foreach (Language current in enumerable)
                {
                    //new Stuff
                    ID languageItemId = LanguageManager.GetLanguageItemId(current, currentItem.Database);

                    Item languageItem = (ItemUtil.IsNull(languageItemId)) ? null: currentItem.Database.GetItem(languageItemId);
                    //Original Stuff
                    //Item languageItem = LanguageManager.GetLanguageItem(current, currentItem.Database);
                    //if (languageItem != null && languageItem.Access.CanRead() && (!languageItem.Appearance.Hidden || UserOptions.View.ShowHiddenItems))
                    
                    //the first condition is new
                    if (ItemUtil.IsNull(languageItemId) || (languageItem != null && languageItem.Access.CanRead() && (!languageItem.Appearance.Hidden || UserOptions.View.ShowHiddenItems)))
                    {
                        XmlControl xmlControl = ControlFactory.GetControl("Gallery.Languages.Option") as XmlControl;
                        Assert.IsNotNull(xmlControl, typeof(XmlControl));
                        Context.ClientPage.AddControl(this.Languages, xmlControl);
                        Item item = currentItem.Database.GetItem(currentItem.ID, current);
                        if (item != null)
                        {
                            int num = item.Versions.GetVersionNumbers(false).Length;
                            string value = (num == 1) ? Translate.Text("1 version.") : Translate.Text("{0} versions.", new object[]
                            {
                                num.ToString()
                            });
                            xmlControl["Icon"] = LanguageService.GetIcon(current, currentItem.Database);
                            System.Globalization.CultureInfo cultureInfo = current.CultureInfo;
                            xmlControl["Header"] = cultureInfo.DisplayName + " : " + cultureInfo.NativeName;
                            xmlControl["Description"] = value;
                            xmlControl["Click"] = string.Format("item:load(id={0},language={1},version=0)", currentItem.ID, current);
                            if (current.Name.Equals(WebUtil.GetQueryString("la"), System.StringComparison.OrdinalIgnoreCase))
                            {
                                xmlControl["ClassName"] = "scMenuPanelItemSelected";
                            }
                            else
                            {
                                xmlControl["ClassName"] = "scMenuPanelItem";
                            }
                        }
                    }
                }
            }
            Context.ClientPage.AddControl(this.Options, new MenuLine());
            Item item2 = Client.CoreDatabase.GetItem("/sitecore/content/Applications/Content Editor/Menues/Languages");
            if (item2 != null)
            {
                this.Options.AddFromDataSource(item2, string.Empty);
            }
            Context.ClientPage.AddControl(this.Options, new MenuLine());
        }
        /// <summary>
        /// Gets the current item.
        /// </summary>
        /// <returns>The current item.</returns>
        private static Item GetCurrentItem()
        {
            string queryString = WebUtil.GetQueryString("db");
            string queryString2 = WebUtil.GetQueryString("id");
            Language language = Language.Parse(WebUtil.GetQueryString("la"));
            Sitecore.Data.Version version = Sitecore.Data.Version.Parse(WebUtil.GetQueryString("vs"));
            Database database = Factory.GetDatabase(queryString);
            Assert.IsNotNull(database, queryString);
            return database.GetItem(queryString2, language, version);
        }
    }
}
