using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using CodeGym.Core.Enums;
using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;
using CodeGym.UI.Helpers;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace CodeGym.UI.Views.Pages;

public partial class ProgressPage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public int CompletedChallenges { get; set; }
    public int CurrentStreak { get; set; }
    public int TotalAttempts { get; set; }
    public double OverallPercent { get; set; }
    public string ProgressText { get; set; } = "";
    public string LastActivityText { get; set; } = "";
    public ObservableCollection<Track> TrackProgress { get; } = new();

    // LiveCharts — Bar chart (progresso por trilha)
    public ISeries[] TrackBarSeries { get; set; } = Array.Empty<ISeries>();
    public Axis[] TrackXAxes { get; set; } = Array.Empty<Axis>();
    public Axis[] TrackYAxes { get; set; } = Array.Empty<Axis>();

    // LiveCharts — Donut chart (visão geral)
    public ISeries[] DonutSeries { get; set; } = Array.Empty<ISeries>();

    public ICommand ExportPdfCommand { get; }

    public ProgressPage()
    {
        ExportPdfCommand = new AsyncRelayCommand(ExportPdfAsync);
        DataContext = this;
        InitializeComponent();
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var attemptRepo = App.Services.GetRequiredService<IAttemptRepository>();
        var challengeRepo = App.Services.GetRequiredService<IChallengeRepository>();

        var progress = await attemptRepo.GetUserProgressAsync();
        CompletedChallenges = progress.CompletedChallenges;
        CurrentStreak = progress.CurrentStreak;
        TotalAttempts = progress.TotalAttempts;

        var total = progress.TotalChallenges;
        OverallPercent = total > 0 ? (double)progress.CompletedChallenges / total * 100 : 0;
        ProgressText = $"{progress.CompletedChallenges} de {total} desafios concluídos";
        LastActivityText = progress.LastAttemptDate.HasValue
            ? $"Última atividade: {progress.LastAttemptDate.Value:dd/MM/yyyy HH:mm}"
            : "Nenhuma atividade registrada";

        var allChallenges = await challengeRepo.GetAllAsync();
        var completedIds = await attemptRepo.GetCompletedChallengeIdsAsync();

        var trackDefs = new[]
        {
            (Type: TrackType.Html, Name: "HTML", Color: new SKColor(0xE4, 0x4D, 0x26)),
            (Type: TrackType.Css, Name: "CSS", Color: new SKColor(0x26, 0x4D, 0xE4)),
            (Type: TrackType.JavaScript, Name: "JavaScript", Color: new SKColor(0xF0, 0xDB, 0x4F)),
            (Type: TrackType.CSharp, Name: "C#", Color: new SKColor(0x68, 0x21, 0x7A)),
        };

        var tracks = new List<Track>();
        foreach (var def in trackDefs)
        {
            var tc = allChallenges.Where(c => c.TrackType == def.Type).ToList();
            var comp = tc.Count(c => completedIds.Contains(c.Id));
            tracks.Add(new Track
            {
                Type = def.Type,
                DisplayName = def.Name,
                TotalChallenges = tc.Count,
                CompletedChallenges = comp
            });
        }

        Dispatcher.Invoke(() =>
        {
            TrackProgress.Clear();
            foreach (var t in tracks) TrackProgress.Add(t);
        });

        // Bar chart — progresso por trilha
        var barValues = new List<double>();
        var barLabels = new List<string>();
        var barColors = new List<SKColor>();

        for (int i = 0; i < tracks.Count; i++)
        {
            barValues.Add(tracks[i].ProgressPercent);
            barLabels.Add(trackDefs[i].Name);
            barColors.Add(trackDefs[i].Color);
        }

        TrackBarSeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Values = barValues.ToArray(),
                Fill = new SolidColorPaint(new SKColor(0x60, 0xCD, 0xFF)),
                MaxBarWidth = 40,
                Padding = 8
            }
        };

        TrackXAxes = new Axis[]
        {
            new Axis
            {
                Labels = barLabels.ToArray(),
                LabelsRotation = 0,
                TextSize = 13,
                LabelsPaint = new SolidColorPaint(new SKColor(0xCC, 0xCC, 0xCC))
            }
        };

        TrackYAxes = new Axis[]
        {
            new Axis
            {
                MinLimit = 0,
                MaxLimit = 100,
                TextSize = 12,
                LabelsPaint = new SolidColorPaint(new SKColor(0xAA, 0xAA, 0xAA)),
                Labeler = v => $"{v:0}%"
            }
        };

        // Donut chart — visão geral
        var completed = progress.CompletedChallenges;
        var remaining = total - completed;

        DonutSeries = new ISeries[]
        {
            new PieSeries<double>
            {
                Values = new double[] { completed },
                Name = "Concluídos",
                Fill = new SolidColorPaint(new SKColor(0x27, 0xAE, 0x60)),
                InnerRadius = 70
            },
            new PieSeries<double>
            {
                Values = new double[] { remaining > 0 ? remaining : 0 },
                Name = "Restantes",
                Fill = new SolidColorPaint(new SKColor(0x55, 0x55, 0x55)),
                InnerRadius = 70
            }
        };

        NotifyAll();
    }

    private async Task ExportPdfAsync()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Exportar Relatório de Progresso",
            Filter = "PDF (*.pdf)|*.pdf",
            DefaultExt = ".pdf",
            FileName = $"CodeGym_Progresso_{DateTime.Now:yyyyMMdd}"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var trackData = TrackProgress.ToList();
            var stats = new
            {
                CompletedChallenges,
                CurrentStreak,
                TotalAttempts,
                OverallPercent,
                LastActivityText
            };

            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header().Column(col =>
                        {
                            col.Item().Text("ProgrammingCraft — Relatório de Progresso")
                                .FontSize(22).Bold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                .FontSize(10).FontColor(Colors.Grey.Medium);
                            col.Item().PaddingBottom(16).LineHorizontal(1)
                                .LineColor(Colors.Grey.Lighten2);
                        });

                        page.Content().Column(col =>
                        {
                            col.Item().PaddingBottom(12).Text("Estatísticas Gerais")
                                .FontSize(16).Bold();

                            col.Item().PaddingBottom(4)
                                .Text($"Desafios concluídos: {stats.CompletedChallenges}");
                            col.Item().PaddingBottom(4)
                                .Text($"Dias de sequência: {stats.CurrentStreak}");
                            col.Item().PaddingBottom(4)
                                .Text($"Total de tentativas: {stats.TotalAttempts}");
                            col.Item().PaddingBottom(4)
                                .Text($"Progresso geral: {stats.OverallPercent:0.0}%");
                            col.Item().PaddingBottom(4)
                                .Text(stats.LastActivityText);

                            col.Item().PaddingVertical(12).LineHorizontal(1)
                                .LineColor(Colors.Grey.Lighten2);

                            col.Item().PaddingBottom(12).Text("Progresso por Trilha")
                                .FontSize(16).Bold();

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Padding(4).Text("Trilha").Bold();
                                    header.Cell().Padding(4).Text("Concluídos").Bold();
                                    header.Cell().Padding(4).Text("Total").Bold();
                                    header.Cell().Padding(4).Text("Progresso").Bold();
                                });

                                foreach (var track in trackData)
                                {
                                    table.Cell().Padding(4).Text(track.DisplayName);
                                    table.Cell().Padding(4).Text($"{track.CompletedChallenges}");
                                    table.Cell().Padding(4).Text($"{track.TotalChallenges}");
                                    table.Cell().Padding(4).Text($"{track.ProgressPercent:0.0}%");
                                }
                            });
                        });

                        page.Footer().AlignCenter()
                            .Text("ProgrammingCraft — Pratique programação 100% offline")
                            .FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                }).GeneratePdf(dialog.FileName);
            });

            System.Windows.MessageBox.Show(
                $"Relatório exportado com sucesso!\n\n{dialog.FileName}",
                "PDF Exportado",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Erro ao exportar PDF: {ex.Message}",
                "Erro",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
        }
    }

    private void Notify([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    private void NotifyAll()
    {
        Notify(nameof(CompletedChallenges));
        Notify(nameof(CurrentStreak));
        Notify(nameof(TotalAttempts));
        Notify(nameof(OverallPercent));
        Notify(nameof(ProgressText));
        Notify(nameof(LastActivityText));
        Notify(nameof(TrackBarSeries));
        Notify(nameof(TrackXAxes));
        Notify(nameof(TrackYAxes));
        Notify(nameof(DonutSeries));
    }
}
