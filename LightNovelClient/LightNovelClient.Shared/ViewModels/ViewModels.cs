﻿using LightNovel.Common;
using LightNovel.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Media;

namespace LightNovel.ViewModels
{
	//[NotifyPropertyChanged]
	public class PropertyChangingEventArgs : EventArgs
	{
		public PropertyChangingEventArgs(string name, object oldValue, object newValue)
		{

		}
		// Summary:
		//     Gets the name of the property that changed.
		//
		// Returns:
		//     The name of the property that changed.
		//public string PropertyName { get; }

		//public object OldValue { get; }
		//public object NewValue { get; }
	}

	public enum PreCachePolicy
	{
		DoNotCache = 0,
		CachePrev = -1,
		CacheNext = 1,
	}

	public class ReadingPageViewModel : INotifyPropertyChanged
	{
		public ReadingPageViewModel()
		{

			//_FontSize = (double)App.Current.Resources["AppFontSizeMediumLarge"];
			//_Background = (SolidColorBrush)App.Current.Resources["AppReadingBackgroundBrush"];
			//_Foreground = (SolidColorBrush)App.Current.Resources["AppForegroundBrush"];
			_FontSize = App.Current.Settings.FontSize;
			_Background = App.Current.Settings.Background;
			_Foreground = App.Current.Settings.Foreground;
			//this.PropertyChanged += ReadingPageViewModel_PropertyChanged;
		}

		//protected override bool HasMoreItemsOverride()
		//{ 
		//		if (ChapterNo < 0 || VolumeData == null)
		//			return false;
		//		return ChapterNo < VolumeData.Chapters.Count - 1;
		//}

		//protected override async Task<IEnumerable<LineViewModel>> LoadMoreItemsOverrideAsync(CancellationToken c, uint count)
		//{
		//	var NextChapterData = await CachedClient.GetChapterAsync(VolumeData.Chapters[ChapterNo + 1].Id);
		//	return _ChapterData.Lines.Select(line => new LineViewModel(line, ChapterData.Id));
		//}


		async void ReadingPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "SeriesId":
					if (SeriesData == null || SeriesId != int.Parse(SeriesData.Id))
					{
						SeriesData = await CachedClient.GetSeriesAsync(SeriesId.ToString());
					}
					break;
				case "VolumeNo":
					if (VolumeData == null || VolumeData.Id != SeriesData.Volumes[VolumeNo].Id)
						VolumeData = SeriesData.Volumes[VolumeNo] = await CachedClient.GetVolumeAsync(SeriesData.Volumes[VolumeNo].Id);
					break;
				case "ChapterNo":
					if (ChapterData == null || ChapterData.Id != VolumeData.Chapters[ChapterNo].Id)
					{
						ChapterData = await CachedClient.GetChapterAsync(VolumeData.Chapters[ChapterNo].Id);
						//VolumeData.Chapters[ChapterNo];
						//CommentIndex = (await LightKindomHtmlClient.GetCommentedLinesListAsync(ChapterNo.ToString())).ToList();
					}
					break;
				case "LineNo":
					break;
				case "PageNo":
					break;
				case "Contents":
					break;
				case "Index":
					break;
				default:
					break;
			}
		}

		public event EventHandler<IEnumerable<int>> CommentsListLoaded;
		//[IgnoreAutoChangeNotification]
		private Series _SeriesData;
		private Volume _VolumeData;
		private Chapter _ChapterData;
		public Series SeriesData
		{
			get
			{
				return _SeriesData;
			}
			private set
			{
				if (_SeriesData != value)
				{
					_SeriesData = value;
					//Index = _SeriesData.Volumes;
					Index = (from vol in _SeriesData.Volumes
							 select new VolumeViewModel
							 {
								 Title = vol.Title,
								 Label = vol.Label,
								 Description = vol.Description,
								 Author = vol.Author,
								 CoverImageUri = new Uri(vol.CoverImageUri),
								 Chapters = vol.Chapters.Select(chapter => new ChapterPreviewModel
								 {
									 No = chapter.ChapterNo,
									 Id = chapter.Id,
									 Title = chapter.Title,
									 VolumeNo = vol.VolumeNo,
								 }).ToList()
							 }).ToList();

					NotifyPropertyChanged("Header");
				}
			}
		}
		//[IgnoreAutoChangeNotification]
		public Volume VolumeData
		{
			get
			{
				return _VolumeData;
			}
			private set
			{
				if (_VolumeData != value)
				{
					_VolumeData = value;
					NotifyPropertyChanged("Header");
					NotifyPropertyChanged("IsFavored");
					NotifyPropertyChanged("IsPinned");

				}
			}
		}

		//[IgnoreAutoChangeNotification]
		public Chapter ChapterData
		{
			get
			{
				return _ChapterData;
			}
			private set
			{
				if (_ChapterData != value)
				{
					_ChapterData = value;
					NotifyPropertyChanged("Header");
				}
			}
		}
		public String Header
		{
			get
			{
#if WINDOWS_APP // USE Long header
				if (SeriesData != null && VolumeData != null && ChapterData != null)
					return SeriesData.Title + " / " + VolumeData.Title + " / " + ChapterData.Title;
				else
					return "Loading...";
#else // WINDOWS_PHONE_APP
				if (SeriesData != null && VolumeData != null && ChapterData != null)
					return VolumeData.Title;
				else
					return "Loading...";
#endif
			}
		}

		private bool _IsLoading = false;
		private int _SeriesId = -1;
		private int _VolumeNo = -1;
		private int _ChapterNo = -1;
		private int _LineNo = -1;
		private int _PageNo = -1;
		private int _PagesCount = -1;
		private IList _Contents;
		private IList<VolumeViewModel> _Index;
		private IList<ChapterPreviewModel> _SeconderyIndex;

		public bool IsLoading
		{
			get { return _IsLoading; }
			set
			{
				if (_IsLoading != value)
				{
					_IsLoading = value;
					NotifyPropertyChanged();
				}
			}
		}
		public bool IsSignedIn
		{
			get
			{
				return App.Current.IsSignedIn;
			}
		}

		public int SeriesId
		{
			get { return _SeriesId; }
			private set
			{
				if (_SeriesId != value)
				{
					_SeriesId = value;
					if (_SeriesId > 0 && (SeriesData == null || SeriesId != int.Parse(SeriesData.Id)))
					{
						throw new NotImplementedException("Set SeriesData before set series ID");
					}
					NotifyPropertyChanged();
				}
			}
		}

		public int VolumeNo
		{
			get { return _VolumeNo; }
			private set
			{
				if (_VolumeNo != value)
				{
					_VolumeNo = value;
					if (_VolumeNo > 0 && (VolumeData == null || VolumeData.Id != SeriesData.Volumes[VolumeNo].Id))
					{
						throw new NotImplementedException("Set VolumeData before set series ID");
					} 
					NotifyPropertyChanged();
				}
			}
		}

		public int ChapterNo
		{
			get { return _ChapterNo; }
			private set
			{
				if (_ChapterNo != value)
				{
					_ChapterNo = value;
#if WINDOWS_APP
					if (Index != null && _VolumeNo >=0 && _ChapterNo >= 0)
					SeconderyIndex = Index[_VolumeNo].Chapters;
#endif
					if (_ChapterNo > 0 && (ChapterData == null || ChapterData.Id != VolumeData.Chapters[ChapterNo].Id))
					{
						throw new NotImplementedException("Set ChapterData before set series ID");
					} 
					
					NotifyPropertyChanged();
					NotifyPropertyChanged("HasPrev");
					NotifyPropertyChanged("HasNext");
				}
			}
		}

		public int LineNo
		{
			get { return _LineNo; }
			set
			{
				if (_LineNo != value)
				{
					_LineNo = value;
					SuppressViewChange = false;
					NotifyPropertyChanged();
				}
			}
		}

		public int PageNo
		{
			get { return _PageNo; }
			set
			{
				if (_PageNo != value)
				{
					_PageNo = value;
					SuppressViewChange = false;
					NotifyPropertyChanged();
				}
			}
		}
		public bool HasPrev
		{
			get
			{
				return (SeriesData != null) && (_ChapterNo > 0);
			}
		}
		public bool HasNext
		{
			get
			{
				return (SeriesData != null) && (VolumeData != null) && (_ChapterNo + 1 < VolumeData.Chapters.Count); ;
			}
		}
		public bool EnableComments
		{
			get
			{
				return App.Current.Settings.EnableComments;
			}
			set
			{
				App.Current.Settings.EnableComments = value;
				NotifyPropertyChanged();
			}
		}

		public int PagesCount
		{
			get { return _PagesCount; }
			set
			{
				if (_PagesCount != value)
				{
					_PagesCount = value;
					NotifyPropertyChanged();
				}
			}
		}

		public IList Contents
		{
			get { return _Contents; }
			private set
			{
				if (_Contents != value)
				{
					_Contents = value;
					NotifyPropertyChanged();
				}
			}
		}

		public IList<VolumeViewModel> Index
		{
			get { return _Index; }
			private set
			{
				if (_Index != value)
				{
					_Index = value;
					NotifyPropertyChanged();
				}
			}
		}

#if WINDOWS_APP

		public IList<ChapterPreviewModel> SeconderyIndex
		{
			get { return _SeconderyIndex; }
			private set
			{
				if (_SeconderyIndex != value)
				{
					_SeconderyIndex = value;
					NotifyPropertyChanged();
				}
			}
		}
#endif

		public bool IsPinned
		{
			get
			{
				if (SeriesId == 0)
					return false;
				return SecondaryTile.Exists(SeriesId.ToString());
			}
		}

		public bool IsFavored
		{
			get
			{
				if (Index == null || !App.Current.IsSignedIn || App.Current.User == null || App.Current.User.FavoriteList == null)
					return false;
				return App.Current.User.FavoriteList.Any(vol => vol.VolumeId == VolumeData.Id);
			}
		}

		public async Task<bool> AddCurrentVolumeToFavoriteAsync()
		{
			if (!App.Current.IsSignedIn)
				return false;
			var result = await App.Current.User.AddUserFavriteAsync(VolumeData,SeriesData.Title);
			if (result)
				NotifyPropertyChanged("IsFavored");
			return result;
		}

		public async Task<bool> RemoveCurrentVolumeFromFavoriteAsync()
		{
			if (!App.Current.IsSignedIn)
				return false; 
			var favol = App.Current.User.FavoriteList.FirstOrDefault(fav => fav.VolumeId == VolumeData.Id);
			if (favol == null)
				return true;
			var result = await App.Current.User.RemoveUserFavriteAsync(favol.FavId);
			if (result)
				NotifyPropertyChanged("IsFavored");
			return result;
		}

		public Task<IEnumerable<string>> GetComentsAsync(int LineNo)
		{
			return LightKindomHtmlClient.GetCommentsAsync(LineNo.ToString(), _ChapterData.Id);
		}

		private Brush _Background;
		private Brush _Foreground;
		private double _FontSize;
		public double FontSize
		{
			get
			{
				return _FontSize;
			}
			set
			{
				if (Math.Abs(_FontSize - value) >= 0.1)
				{
					_FontSize = value;
					NotifyPropertyChanged();
					App.Current.Settings.FontSize = value;
				}
			}
		}
		public Brush Foreground
		{
			get
			{
				return _Foreground;
			}
			set
			{
				_Foreground = value;
				NotifyPropertyChanged();
				App.Current.Settings.Foreground = value;
			}
		}
		public Brush Background
		{
			get
			{
				return _Background;
			}
			set
			{
				_Background = value;
				NotifyPropertyChanged();
				App.Current.Settings.Background = value;
			}
		}

		public async Task LoadDataAsync(NovelPositionIdentifier nav)
		{
			if (IsLoading)
				return;
			IsLoading = true;
			if (nav.ChapterNo < 0) nav.ChapterNo = 0;
			if (nav.VolumeNo < 0) nav.VolumeNo = 0;
			if (nav.ChapterId != null && nav.VolumeId == null && nav.SeriesId == null)
			{
				try
				{
					if (nav.SeriesId == null)
					{
						var chapter = await CachedClient.GetChapterAsync(nav.ChapterId);
						nav.SeriesId = chapter.ParentSeriesId;
						nav.VolumeId = chapter.ParentVolumeId;
					}
					var series = await CachedClient.GetSeriesAsync(nav.SeriesId);
					var volume = series.Volumes.First(vol => vol.Id == nav.VolumeId);
					nav.VolumeNo = series.Volumes.IndexOf(volume);
					nav.ChapterNo = volume.Chapters.IndexOf(volume.Chapters.First(cpt => cpt.Id == nav.ChapterId));
				}
				catch (Exception exception)
				{
					Debug.WriteLine("Error in converting navigator data : {0}", exception.Message);
					return;
				}
			}
			else if (nav.VolumeId != null && nav.SeriesId == null)
			{
				try
				{
					string volDesc = null;
					if (nav.SeriesId == null)
					{
						var volume = await CachedClient.GetVolumeAsync(nav.VolumeId);
						nav.SeriesId = volume.ParentSeriesId;
						volDesc = volume.Description;
					}
					SeriesData = await CachedClient.GetSeriesAsync(nav.SeriesId);
					SeriesId = int.Parse(nav.SeriesId);
					VolumeData = SeriesData.Volumes.FirstOrDefault(vol => vol.Id == nav.VolumeId);
					if (VolumeData != null)
					{
						nav.VolumeNo = SeriesData.Volumes.IndexOf(VolumeData);
						VolumeNo = nav.VolumeNo;
					}
					else // This is the case that we need to refresh the series data!
					{
						SeriesData = await CachedClient.GetSeriesAsync(nav.SeriesId, true);
						VolumeData = SeriesData.Volumes.FirstOrDefault(vol => vol.Id == nav.VolumeId);
						nav.VolumeNo = SeriesData.Volumes.IndexOf(VolumeData);
						VolumeNo = nav.VolumeNo;
					}
				}
				catch (Exception exception)
				{
					Debug.WriteLine("Error in converting navigator data : {0}", exception.Message);
					Contents = new LineViewModel[] { new LineViewModel(1,"Failed to resolve data navigator :("),
										new LineViewModel(2,"Please contact Check your Internet connection."),
										new LineViewModel(3,"Exception detail : " + exception.Message) };
					IsLoading = false;
					LineNo = 0;
					return;
				}
			}
			await LoadDataAsync(int.Parse(nav.SeriesId), nav.VolumeNo, nav.ChapterNo, nav.LineNo);

		}

		public async Task LoadDataAsync(int seriesId, int volumeNo = 0, int chapterNo = 0, int lineNo = 0, PreCachePolicy preCachePolicy = PreCachePolicy.CacheNext)
		{
			IsLoading = true;
			try
			{
				if (seriesId > 0 && (SeriesData == null || seriesId != int.Parse(SeriesData.Id)))
				{
					SeriesData = await CachedClient.GetSeriesAsync(seriesId.ToString());
					SeriesId = seriesId;
				}
				if (volumeNo >= 0 && (VolumeData == null || VolumeData.Id != SeriesData.Volumes[volumeNo].Id))
				{
					VolumeData = SeriesData.Volumes[volumeNo];// = await CachedClient.GetVolumeAsync(SeriesData.Volumes[volumeNo.Value].Id);
					VolumeNo = volumeNo;
					var task = CachedClient.GetChapterAsync(VolumeData.Chapters[0].Id);
					//NotifyPropertyChanged("Index");
				}
				if (chapterNo >= 0 && (ChapterData == null || ChapterData.Id != VolumeData.Chapters[chapterNo].Id))
				{
					var chapter = await CachedClient.GetChapterAsync(VolumeData.Chapters[chapterNo].Id);

					// Fix for cached page leads to no content
					if (chapter.Lines.Count == 0)
						chapter = await CachedClient.GetChapterAsync(VolumeData.Chapters[chapterNo].Id,true);

					chapter.Title = VolumeData.Chapters[chapterNo].Title;
					ChapterData = chapter; //VolumeData.Chapters[chapterNo.Value] =
					ChapterNo = chapterNo;

					if (preCachePolicy == PreCachePolicy.CacheNext && HasNext)
					{
						// Pre caching next chapter
						CachedClient.GetChapterAsync(VolumeData.Chapters[chapterNo+1].Id);
					} else if (preCachePolicy == PreCachePolicy.CachePrev && HasPrev)
					{
						CachedClient.GetChapterAsync(VolumeData.Chapters[chapterNo - 1].Id);
					}

					var lvms = _ChapterData.Lines.Select(line => new LineViewModel(line,ChapterData.Id));
					//_storage.AddRange(lvms);
					//NotifyOfInsertedItems(0, _storage.Count);
					//NotifyPropertyChanged("Contents");
					Contents = lvms.ToList();
					if (Contents.Count == 0)
					{
						Contents = new LineViewModel[] { new LineViewModel(1,"Failed to load data :("),
										new LineViewModel(2,"Sorry, the content you request is currently unavailable.")};
					}
					//var collection = new PagelizedIncrementalVector<LineViewModel>(ChapterNo,new List<int>(VolumeData.Chapters.Select(c=>0)), lvms);
					//collection.AccuirePageData = async chptNo =>
					//{
					//	return (await CachedClient.GetChapterAsync(VolumeData.Chapters[chptNo].Id))
					//		.Lines.Select(line => new LineViewModel(line, ChapterData.Id)); 
					//};

					//collection.CollectionChanged += collection_CollectionChanged;

					//Contents = collection;
					//SeconderyIndex = VolumeData.Chapters;
					//NotifyPropertyChanged("SeconderyIndex");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				Contents = new LineViewModel[] { new LineViewModel(1,"Failed to load data :("),
										new LineViewModel(2,"Please check your Internet Connectivity."),
										new LineViewModel(3,"Exception detail : " + ex.Message) };
				IsLoading = false;
				LineNo = 0;
				return;
			}

			IsLoading = false;


			if (lineNo >= Contents.Count)
				lineNo = 0;
			if (lineNo >= 0)
				LineNo = lineNo;
			else
				LineNo = Contents.Count + lineNo;

			var bookmark = CreateBookmark();
			await App.Current.UpdateHistoryListAsync(bookmark);
			await App.UpdateSecondaryTileAsync(bookmark);

			if (EnableComments && Contents!=null)
			{
				try
				{
					var CommentsList = await LightKindomHtmlClient.GetCommentedLinesListAsync(ChapterData.Id);
					foreach (var cln in CommentsList)
					{
						((LineViewModel)Contents[cln - 1]).MarkAsCommented();
					}
					if (CommentsListLoaded != null)
						CommentsListLoaded.Invoke(this, CommentsList);
				}
				catch (Exception exception)
				{
					Debug.WriteLine(exception);
				}

			}
		}

		void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Debug.WriteLine(String.Format("Contents Collection size changed, Count = {0}", Contents.Count));
		}

		public bool IsDataLoaded
		{
			get
			{
				return ChapterData != null && VolumeData != null && SeriesData != null;
			}
		}

		public BookmarkInfo CreateBookmark()
		{
			var bookmark = new BookmarkInfo
			{
				ViewDate = DateTime.Now,
				Position = new NovelPositionIdentifier
				{
					ChapterNo = ChapterNo,
					VolumeNo = VolumeNo,
					SeriesId = SeriesId.ToString(),//App.CurrentSeries.Id,
					LineNo = LineNo,
				},
				Progress = Contents.Count != 0 ? (LineNo / Contents.Count) : 0,
			};

			if (IsDataLoaded)
			{
				bookmark.ChapterTitle = ChapterData.Title;
				bookmark.VolumeTitle = VolumeData.Title;
				bookmark.SeriesTitle = SeriesData.Title;
				bookmark.DescriptionThumbnailUri = VolumeData.CoverImageUri;
				bookmark.DescriptionImageUri = VolumeData.CoverImageUri;

				if (CachedClient.ChapterCache.ContainsKey(VolumeData.Chapters[0].Id) && CachedClient.ChapterCache[VolumeData.Chapters[0].Id].IsCompleted && !CachedClient.ChapterCache[VolumeData.Chapters[0].Id].IsFaulted)
				{
					// Find the First Illustration of current Volume
					var imageLine = CachedClient.ChapterCache[VolumeData.Chapters[0].Id].Result.Lines.FirstOrDefault(line => line.ContentType == LineContentType.ImageContent);
					if (imageLine != null)
						bookmark.DescriptionImageUri = imageLine.Content;
				}
				else
				{
					var imageLine = Contents.Cast<LineViewModel>().FirstOrDefault(line => line.ContentType == LineContentType.ImageContent);
					if (imageLine != null)
						bookmark.DescriptionImageUri = imageLine.Content;
				}

				var textLines = from line in Contents.Cast<LineViewModel>()
								where line.ContentType == LineContentType.TextContent && line.No >= LineNo && line.No <= LineNo + 5
								select line.Content;
				var builder = new StringBuilder();
				bookmark.ContentDescription = textLines.Aggregate(builder, (b, s) => { b.AppendLine(s); return b; }).ToString();
			}
			else
			{
				bookmark.ContentDescription = "Failed to load data, please try again when you connected";
			}
			return bookmark;
		}

		// When LineNo/PageNo Changed by UI Report, this flag will be TRUE, When User set, this will be FALSE
		public bool SuppressViewChange { get; set; }
		// Use this method to set the LineNo and PageNo without calling NotifyPropertyChanged Event
		public void ReportViewChanged(int? pageNo, int? lineNo = null)
		{
			SuppressViewChange = true;
			if (lineNo != null)
			{
				_LineNo = lineNo.Value;
				NotifyPropertyChanged("LineNo");
			}
			if (pageNo != null)
			{
				_PageNo = pageNo.Value;
				NotifyPropertyChanged("PageNo");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	public class BookCoverViewModel : INotifyPropertyChanged
	{
		public BookCoverViewModel()
		{ }

		public BookCoverViewModel(BookItem item)
		{
			Title = item.Title;
			Subtitle = item.Subtitle;
			Id = item.Id;
			CoverImageUri = item.CoverImageUri;
			ItemType = item.ItemType;
		}

		string _title;
		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				NotifyPropertyChanged();
			}
		}
		string _subtitle;
		public string Subtitle
		{
			get { return _subtitle; }
			set
			{
				_subtitle = value;
				NotifyPropertyChanged();
			}
		}

		public async Task LoadDescriptionAsync()
		{
			if (ItemType == BookItemType.Volume)
			{
				var vol = await CachedClient.GetVolumeAsync(Id);
				Description = vol.Description;
			}
			else if (ItemType == BookItemType.Series)
			{
				var ser = await CachedClient.GetSeriesAsync(Id);
				Description = ser.Description;
			}

		}

		string _description;
		public string Description
		{
			get { return _description; }
			set
			{
				_description = value;
				NotifyPropertyChanged();
			}
		}

		string _id;
		public string Id
		{
			get { return _id; }
			set
			{
				_id = value;
				NotifyPropertyChanged();
			}
		}

		public string NavigateUri
		{
			get
			{
				switch (ItemType)
				{
					case BookItemType.Series:
						return "/SeriesViewPage.xaml?id=" + Id;
					case BookItemType.Volume:
						return String.Format("/SeriesViewPage.xaml?id={0}&volume={1}", "", Id);
					case BookItemType.Chapter:
						return "/ChapterViewPage.xaml?id=" + Id;
				}

				return "/";
			}
		}

		public BookItemType ItemType { get; set; }

		public string CoverImageUri
		{
			get { return _coverUri; }
			set
			{
				_coverUri = value;
				NotifyPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private string _coverUri;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	public class SeriesPreviewModel : INotifyPropertyChanged
	{
		string _title;
		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				NotifyPropertyChanged();
			}
		}
		string _id;
		public string ID
		{
			get { return _id; }
			set
			{
				_id = value;
				NotifyPropertyChanged();
			}
		}

		public string NavigateUri
		{
			get { return "/SeriesViewPage.xaml?id=" + ID; }
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}


	public class HistoryItemViewModel : INotifyPropertyChanged
	{
		public HistoryItemViewModel() { }
		public HistoryItemViewModel(BookmarkInfo item){
			Position = item.Position;
			ProgressPercentage = item.Progress;
			CoverImageUri = item.DescriptionImageUri;
			Description = item.ContentDescription;
			ChapterTitle = item.ChapterTitle;
			VolumeTitle = item.VolumeTitle;
			SeriesTitle = item.SeriesTitle;
			UpdateTime = item.ViewDate;
		}
		public NovelPositionIdentifier Position { get; set; }

		public double ProgressPercentage { get; set; }
		public string NavigateUri
		{
			get { return String.Format("/ChapterViewPage.xaml?id={0}&line={1}", Position.ChapterId, Position.LineNo); }
		}
		public DateTime UpdateTime { get; set; }

		private string _coverImageUri;
		public string CoverImageUri
		{
			get
			{
				return _coverImageUri;
			}
			set
			{
				if (_coverImageUri != value)
				{
					_coverImageUri = value;
					NotifyPropertyChanged();
				}
			}
		}
		private string _description;
		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				if (_description != value)
				{
					_description = value;
					NotifyPropertyChanged();
				}
			}
		}
		public string SeriesTitle { get; set; }
		public string VolumeTitle { get; set; }
		public string ChapterTitle { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	public class SeriesViewModel : KeyGroup<string,VolumeViewModel>, INotifyPropertyChanged 
	{
		private string _title;
		private string _author;
		private string _illustrator;
		private string _publisher;
		private DateTime _updateTime;
		private string _description;
		private Uri _coverImageUri;
		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				NotifyPropertyChanged();
			}
		}
		public string Author
		{
			get { return _author; }
			set
			{
				_author = value;
				NotifyPropertyChanged();
			}
		}
		public string Illustrator
		{
			get { return _illustrator; }
			set
			{
				_illustrator = value;
				NotifyPropertyChanged();
			}
		}
		public string Publisher
		{
			get { return _publisher; }
			set
			{
				_publisher = value;
				NotifyPropertyChanged();
			}
		}
		public DateTime UpdateTime
		{
			get { return _updateTime; }
			set
			{
				_updateTime = value;
				NotifyPropertyChanged();
			}
		}
		public string Description
		{
			get { return _description; }
			set
			{
				_description = value;
				NotifyPropertyChanged();
			}
		}
		public Uri CoverImageUri
		{
			get { return _coverImageUri; }
			set
			{
				if (_coverImageUri != value)
				{
					_coverImageUri = value;
					NotifyPropertyChanged();
				}
			}
		}

		public string Id
		{
			get;
			set;
		}
		public SeriesViewModel()
		{
			_isLoading = false;
		}

		private bool _isLoading;
		public bool IsLoading
		{
			get { return _isLoading; }
			set
			{
				_isLoading = value;
				NotifyPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	public class VolumeViewModel : IList<ChapterPreviewModel>, INotifyPropertyChanged
	{

		#region Self Properties
		private string _title;
		private string _author;
		private string _illustrator;
		private string _publisher;
		private DateTime _updateTime;
		private string _description;
		private Uri _coverImageUri;
		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				NotifyPropertyChanged();
			}
		}
		public string Author
		{
			get { return _author; }
			set
			{
				_author = value;
				NotifyPropertyChanged();
			}
		}
		public string Illustrator
		{
			get { return _illustrator; }
			set
			{
				_illustrator = value;
				NotifyPropertyChanged();
			}
		}
		public string Publisher
		{
			get { return _publisher; }
			set
			{
				_publisher = value;
				NotifyPropertyChanged();
			}
		}
		public DateTime UpdateTime
		{
			get { return _updateTime; }
			set
			{
				_updateTime = value;
				NotifyPropertyChanged();
			}
		}
		public string Description
		{
			get { return _description; }
			set
			{
				//if (value == "" || value == null)
				//	_description = AppResources.NoDescription;
				_description = value;
				NotifyPropertyChanged();
			}
		}
		public Uri CoverImageUri
		{
			get { return _coverImageUri; }
			set
			{
				if (_coverImageUri != value)
				{
					_coverImageUri = value;
					NotifyPropertyChanged();
				}
			}
		}

		public Uri FirstIllustrationUri
		{
			get;
			set;
		}

		public IList<ChapterPreviewModel> Chapters { get; set; }

		private bool _isLoading;
		public bool IsLoading
		{
			get { return _isLoading; }
			set
			{
				if (_isLoading == value) return;
				_isLoading = value;
				NotifyPropertyChanged();
			}
		}
		#endregion


		#region IList

		public void Add(ChapterPreviewModel value)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(ChapterPreviewModel value)
		{
			Debug.WriteLine("Contains Get Called");
			return Chapters.Contains(value);
		}

		public int IndexOf(ChapterPreviewModel value)
		{
			Debug.WriteLine("IndexOf Get Called");
			return Chapters.IndexOf(value);
		}

		public void Insert(int index, ChapterPreviewModel value)
		{
			throw new NotImplementedException();
		}

		public bool IsFixedSize
		{
			get { return true; }
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		public bool Remove(ChapterPreviewModel value)
		{
			throw new NotImplementedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotImplementedException();
		}

		public ChapterPreviewModel this[int index]
		{
			get
			{
				if (index >= Chapters.Count)
					Debug.WriteLine("Shit! Index out of range at : " + index);
				//Debug.WriteLine(String.Format("this[{0}]",index));
				return Chapters[index];
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public void CopyTo(ChapterPreviewModel[] array, int index)
		{
			//Debug.WriteLine("CopyTo Called");
			((IList)Chapters).CopyTo(array, index);
		}

		public int Count
		{
			get
			{
				Debug.WriteLine("Count Get Called");
				return Chapters.Count;
			}
		}

		public bool IsSynchronized
		{
			get { return false; }
		}

		public object SyncRoot
		{
			get { throw new NotImplementedException(); }
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			Debug.WriteLine("GetEnumerator() Get Called");
			return Chapters.GetEnumerator();
		}
		public IEnumerator<ChapterPreviewModel> GetEnumerator()
		{
			Debug.WriteLine("GetEnumerator() Get Called");
			return Chapters.GetEnumerator();
		}
		#endregion 

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public string Id { get; set; }

		public string Label { get; set; }
	}


	public class ChapterPreviewModel : INotifyPropertyChanged
	{
		string _title;
		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				NotifyPropertyChanged();
			}
		}
		string _id;
		public string Id
		{
			get { return _id; }
			set
			{
				_id = value;
				NotifyPropertyChanged();
			}
		}
		int _no;
		public int No
		{
			get { return _no; }
			set
			{
				_no = value;
				NotifyPropertyChanged();
			}
		}

		public int VolumeNo
		{
			get;
			set;
		}

		public bool IsDownloaded { get; set; }
		public string NavigateUri
		{
			get { return "/ChapterViewPage.xaml?id=" + Id; }
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	public class Comment : INotifyPropertyChanged
	{
		private string _author;
		private DateTime _date;
		private string _content;

		public Comment()
		{
			_content = "uninitialized";
			_author = "unknown";
		}

		public Comment(string content)
		{
			_content = content;
			_date = DateTime.MinValue;
			_author = null;
		}

		public string Content
		{
			get { return _content; }
			set
			{
				_content = value;
				NotifyPropertyChanged();
			}
		}

		public DateTime Date
		{
			get { return _date; }
			set
			{
				_date = value;
				NotifyPropertyChanged();
			}
		}

		public string Author
		{
			get { return _author; }
			set
			{
				_author = value;
				NotifyPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	public class LineViewModel : INotifyPropertyChanged
	{
		private Task<IEnumerable<string>> LoadCommentTask;
		private string _content;
		private bool _isLoading;
		private ObservableCollection<Comment> _comments;

		public string ParentChapterId { get; set; }

		public LineContentType ContentType
		{
			get
			{
				if (Uri.IsWellFormedUriString(_content, UriKind.Absolute))
					return LineContentType.ImageContent;
				return LineContentType.TextContent;
			}
		}

		public int No { get; set; }

		public LineViewModel()
		{
			_content = null;
		}

		public LineViewModel(Line line,string chapterId)
		{
			//if (line.ContentType == LineContentType.TextContent)
			//	_content = line.Content; // Add the indent
			//else

			IsImage = line.ContentType == LineContentType.ImageContent;
#if WINDOWS_PHONE_APP
			if (!IsImage)
				_content = "　" + line.Content; // Indent
			else
				_content = line.Content;
#endif
			ParentChapterId = chapterId;
			No = line.No;
		}

		public Uri ImageUri
		{
			get
			{
				if (!_content.StartsWith("http://"))
					return null;
				return new Uri(_content, UriKind.Absolute);
			}
		}

		public LineViewModel(string content)
		{
			_content = content;
		}
		public LineViewModel(int id, string content)
		{
			No = id;
			_content = content;
			IsImage = false;
		}
		public LineViewModel(string content, params string[] comments)
		{
			_content = content;
			Comments = new ObservableCollection<Comment>(comments.Select(comment => new Comment(comment)));
			Comments.CollectionChanged += Comments_CollectionChanged;
		}

		public void MarkAsCommented()
		{
			if (Comments != null)
				return;
			Comments = new ObservableCollection<Comment>();
			Comments.CollectionChanged += Comments_CollectionChanged;
		}

		public async Task<bool> AddCommentAsync(string commentText)
		{
			if (!string.IsNullOrEmpty(commentText) && commentText.Length < 300 && !String.IsNullOrEmpty(ParentChapterId))
			{
				if (HasNoComment)
					MarkAsCommented();
				Comments.Add(new Comment(commentText));
				return await LightKindomHtmlClient.CreateCommentAsync(No.ToString(), ParentChapterId, commentText);
			}
			return false;
		}

		void Comments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			NotifyPropertyChanged("CommentsNotice");
		}

		public virtual string Content
		{
			get { return _content; }
			set
			{
				_content = value;
				NotifyPropertyChanged("Content");
			}
		}

		public string CommentsNotice
		{
			get
			{
				if (Comments != null)
					return String.Format("{0} comments", Comments.Count);
				else
					return null;
			}
		}

		public bool IsLoading
		{
			get { return _isLoading; }
			set
			{
				if (_isLoading == value) return;
				_isLoading = value;
				NotifyPropertyChanged("IsLoading");
			}
		}

		public bool HasComments
		{
			get { return Comments != null; }
		}

		public bool HasNoComment
		{
			get { return Comments == null; }
		}

		public bool IsImage
		{
			get;
			set;
			//get { return Uri.IsWellFormedUriString(_content, UriKind.Absolute); }
		}

		public ObservableCollection<Comment> Comments
		{
			get { return _comments; }
			set
			{
				if (value != _comments)
				{
					_comments = value;
					NotifyPropertyChanged("Comments");
					NotifyPropertyChanged("HasComments");
					NotifyPropertyChanged("HasNoComments");
				}
			}
		}

		public async Task LoadCommentsAsync()
		{
			if (HasNoComment || String.IsNullOrEmpty(ParentChapterId) || IsLoading) 
				return;
			if (LoadCommentTask == null && Comments.Count == 0)
			{
				string lineId = No.ToString();
				Debug.WriteLine("Loading Comments : line_id = " + lineId + " ,chapter_id = " + ParentChapterId);
				LoadCommentTask = LightKindomHtmlClient.GetCommentsAsync(lineId, ParentChapterId);
				IsLoading = true;
				try
				{
					var comments = await LoadCommentTask;
					foreach (var comment in comments)
					{
						Comments.Add(new Comment(comment));
					}
					LoadCommentTask = null;
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Comments load failed : line_id = " + lineId + " ,chapter_id = " + ParentChapterId + "exception : " + ex.Message);
				}
				IsLoading = false;
			}
			return;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(String info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}

	}

}