// <copyright file="WorkbookViewModel.cs" company="WillowTree, LLC">
// Copyright (c) WillowTree, LLC. All rights reserved.
// </copyright>

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;
using WillowTree.Sweetgum.Client.Workbooks.Models;
using WillowTree.Sweetgum.Client.Workbooks.Services;

namespace WillowTree.Sweetgum.Client.Workbooks.ViewModels
{
    /// <summary>
    /// A workbook view model.
    /// </summary>
    public sealed class WorkbookViewModel : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<bool> isRenamingObservableAsPropertyHelper;
        private string name;
        private WorkbookItemsViewModel workbookItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookViewModel"/> class.
        /// </summary>
        /// <param name="workbookModel">An instance of <see cref="WorkbookModel"/>.</param>
        public WorkbookViewModel(WorkbookModel workbookModel)
        {
            this.name = workbookModel.Name;

            var isRenamingBehaviorSubject = new BehaviorSubject<bool>(false);

            this.isRenamingObservableAsPropertyHelper = isRenamingBehaviorSubject
                .ToProperty(this, viewModel => viewModel.IsRenaming);

            this.RenameCommand = ReactiveCommand.Create(() => isRenamingBehaviorSubject.OnNext(true));
            this.FinishRenameCommand = ReactiveCommand.Create(() =>
            {
                isRenamingBehaviorSubject.OnNext(false);
                workbookModel = workbookModel.Rename(this.Name);
            });

            this.SaveCommand = ReactiveCommand.CreateFromTask(
                async (cancellationToken) =>
                {
                    await WorkbookManager.SaveAsync(workbookModel, cancellationToken);
                    return Unit.Default;
                },
                isRenamingBehaviorSubject.Select(r => !r));

            this.NewFolderInteraction = new Interaction<WorkbookModel, string?>();

            this.NewFolderCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var path = await this.NewFolderInteraction.Handle(workbookModel);

                if (path != null)
                {
                    workbookModel = workbookModel.NewFolder(path);
                    this.WorkbookItems = new WorkbookItemsViewModel(workbookModel.Folders);
                }

                return Unit.Default;
            });

            this.workbookItems = new WorkbookItemsViewModel(workbookModel.Folders);
        }

        /// <summary>
        /// Gets or sets the name of the workbook.
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Gets a value indicating whether or not the workbook is being renamed.
        /// </summary>
        public bool IsRenaming => this.isRenamingObservableAsPropertyHelper.Value;

        /// <summary>
        /// Gets a command to rename the workbook.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RenameCommand { get; }

        /// <summary>
        /// Gets a command to finish renaming the workbook.
        /// </summary>
        public ReactiveCommand<Unit, Unit> FinishRenameCommand { get; }

        /// <summary>
        /// Gets a command to create a new folder in the workbook.
        /// </summary>
        public ReactiveCommand<Unit, Unit> NewFolderCommand { get; }

        /// <summary>
        /// Gets an interaction to prompt to the user for creating a new folder in the workbook.
        /// </summary>
        public Interaction<WorkbookModel, string?> NewFolderInteraction { get; }

        /// <summary>
        /// Gets a command to save the workbook.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

        /// <summary>
        /// Gets or sets the workbook items.
        /// </summary>
        public WorkbookItemsViewModel WorkbookItems
        {
            get => this.workbookItems;
            set => this.RaiseAndSetIfChanged(ref this.workbookItems, value);
        }
    }
}
