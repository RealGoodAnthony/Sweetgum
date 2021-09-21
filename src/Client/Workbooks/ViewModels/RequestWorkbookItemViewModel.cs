// <copyright file="RequestWorkbookItemViewModel.cs" company="WillowTree, LLC">
// Copyright (c) WillowTree, LLC. All rights reserved.
// </copyright>

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;
using WillowTree.Sweetgum.Client.Requests.Models;
using WillowTree.Sweetgum.Client.Workbooks.Models;

namespace WillowTree.Sweetgum.Client.Workbooks.ViewModels
{
    /// <summary>
    /// A request workbook item view model.
    /// </summary>
    public sealed class RequestWorkbookItemViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<int> levelObservableAsPropertyHelper;
        private readonly ObservableAsPropertyHelper<RequestModel> requestModelObservableAsPropertyHelper;
        private readonly ObservableAsPropertyHelper<PathModel> originalPathObservableAsPropertyHelper;
        private readonly BehaviorSubject<RequestModel> requestModelBehaviorSubject;
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestWorkbookItemViewModel"/> class.
        /// </summary>
        /// <param name="requestModel">An instance of <see cref="RequestModel"/>.</param>
        /// <param name="openRequestCommand">A command to invoke to open the request.</param>
        public RequestWorkbookItemViewModel(
            RequestModel requestModel,
            ReactiveCommand<RequestModel, Unit> openRequestCommand)
        {
            this.name = requestModel.Name;

            this.OpenRequestCommand = openRequestCommand;

            this.requestModelBehaviorSubject = new BehaviorSubject<RequestModel>(requestModel);

            this.levelObservableAsPropertyHelper =
                this.requestModelBehaviorSubject
                    .Select(CalculateLevel)
                    .ToProperty(this, viewModel => viewModel.Level);

            this.requestModelObservableAsPropertyHelper =
                this.requestModelBehaviorSubject.ToProperty(this, viewModel => viewModel.RequestModel);

            this.originalPathObservableAsPropertyHelper =
                this.requestModelBehaviorSubject
                    .Select(newRequestModel => newRequestModel.GetPath())
                    .ToProperty(this, viewModel => viewModel.OriginalPath);
        }

        /// <summary>
        /// Gets or sets the name of the folder.
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Gets the original path model for this request.
        /// </summary>
        public PathModel OriginalPath => this.originalPathObservableAsPropertyHelper.Value;

        /// <summary>
        /// Gets the open request command.
        /// </summary>
        public ReactiveCommand<RequestModel, Unit> OpenRequestCommand { get; }

        /// <summary>
        /// Gets the level of the request, which is the depth in the workbook.
        /// </summary>
        public int Level => this.levelObservableAsPropertyHelper.Value;

        /// <summary>
        /// Gets the request model of the workbook item.
        /// </summary>
        public RequestModel RequestModel => this.requestModelObservableAsPropertyHelper.Value;

        /// <summary>
        /// Update a request using a new request model and new workbook model.
        /// </summary>
        /// <param name="requestModel">The new request model.</param>
        public void Update(
            RequestModel requestModel)
        {
            this.Name = requestModel.Name;
            this.requestModelBehaviorSubject.OnNext(requestModel);
        }

        private static int CalculateLevel(RequestModel requestModel)
        {
            return requestModel.GetPath().Segments.Count - 1;
        }
    }
}
