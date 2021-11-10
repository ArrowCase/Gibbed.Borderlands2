/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Gibbed.Borderlands2.GameInfo;
using Gibbed.Borderlands2.ProtoBufFormats.WillowTwoSave;

namespace Gibbed.Borderlands2.SaveEdit
{
    [Export(typeof(MissionsViewModel))]
    internal class MissionsViewModel : PropertyChangedBase
    {
        private int _selectedPlaythrough;
        public int SelectedPlaythrough
        {
            get
            {
                return this._selectedPlaythrough;
            }
            set
            {
                this._selectedPlaythrough = value;
                this.NotifyOfPropertyChange(nameof(SelectedPlaythrough));
                ShowTotal();
                ShowMission();
            }
        }

        private int _selectedMission;
        public int SelectedMission
        {
            get
            {
                return this._selectedMission;
            }
            set
            {
                this._selectedMission = value;
                this.NotifyOfPropertyChange(nameof(SelectedMission));
                ShowMission();
            }
        }

        private int _totalMissions;
        public int TotalMissions
        {
            get
            {
                return this._totalMissions;
            }
            set
            {
                this._totalMissions = value;
                this.NotifyOfPropertyChange(nameof(TotalMissions));
            }
        }

        private string _totalLabel;
        public string TotalLabel
        {
            get
            {
                return this._totalLabel;
            }
            set
            {
                this._totalLabel = value;
                this.NotifyOfPropertyChange(nameof(TotalLabel));
            }
        }

        public ObservableCollection<StoryPlaythrough> StoryPlaythroughs { get; private set; }
        public List<MissionPlaythroughData> PlaythroughData { get; private set; }
        public ObservableCollection<MissionData> NormalMissions { get; private set; }
        public ObservableCollection<MissionData> TvhmMissions { get; private set; }
        public ObservableCollection<MissionData> UvhmMissions { get; private set; }

        public ObservableCollection<Mission> MissionList { get; private set; }

        private void ShowTotal()
        {
            /*int total = 0;
            if (SelectedPlaythrough == 0)
            {
                total = NormalMissions?.Count ?? 0;
            }
            else if (SelectedPlaythrough == 1)
            {
                total = TvhmMissions?.Count ?? 0;
            }
            else if (SelectedPlaythrough == 2)
            {
                total = UvhmMissions?.Count ?? 0;
            }
            TotalLabel = $"{total} of {TotalMissions}";*/
            var lines = new List<string>();
            if (NormalMissions != null)
            {
                foreach (var mission in MissionList)
                {
                    if (NormalMissions.FirstOrDefault(m => m.Mission == mission.Id) == null)
                    {
                        lines.Add($"NVHM: missing: {mission.Name}");
                    }
                }
                foreach (var miss in NormalMissions.Where(m => m.Status != ProtoBufFormats.WillowTwoSave.MissionStatus.Complete))
                {
                    var mission = MissionList.First(m => m.Id == miss.Mission);
                    lines.Add($"NVHM: active: {mission.Name}");
                }
            }
            if (TvhmMissions != null)
            {
                foreach (var mission in MissionList)
                {
                    if (TvhmMissions.FirstOrDefault(m => m.Mission == mission.Id) == null)
                    {
                        lines.Add($"TVHM: missing: {mission.Name}");
                    }
                }
                foreach (var miss in TvhmMissions.Where(m => m.Status != ProtoBufFormats.WillowTwoSave.MissionStatus.Complete))
                {
                    var mission = MissionList.First(m => m.Id == miss.Mission);
                    lines.Add($"TVHM: active: {mission.Name}");
                }
            }
            if (UvhmMissions != null)
            {
                foreach (var mission in MissionList)
                {
                    if (UvhmMissions.FirstOrDefault(m => m.Mission == mission.Id) == null)
                    {
                        lines.Add($"UVHM: missing: {mission.Name}");
                    }
                }
                foreach (var miss in UvhmMissions.Where(m => m.Status != ProtoBufFormats.WillowTwoSave.MissionStatus.Complete))
                {
                    var mission = MissionList.First(m => m.Id == miss.Mission);
                    lines.Add($"UVHM: active: {mission.Name}");
                }
            }
            TotalLabel = string.Join(System.Environment.NewLine, lines);
            System.IO.File.WriteAllLines("missioncomp.txt", lines);
        }

        private void ShowMission()
        {

        }

        [ImportingConstructor]
        public MissionsViewModel()
        {
            var playthroughs = new List<StoryPlaythrough>()
            {
                new StoryPlaythrough(PlaythroughKind.Normal),
                new StoryPlaythrough(PlaythroughKind.TVHM),
                new StoryPlaythrough(PlaythroughKind.UVHM)
            };

            StoryPlaythroughs = new ObservableCollection<StoryPlaythrough>(playthroughs);

            var missions = InfoManager.Missions
                .Items
                .Where(kv => kv.Value is Mission)
                .Select(kv => kv.Value)
                .Cast<Mission>()
                .OrderBy(m => m.Number)
                .ToList();

            TotalMissions = missions.Count;

            var ml = missions.FirstOrDefault(mm => mm.Name == "Treasure of the Sands");
            System.Console.WriteLine(ml);

            MissionList = new ObservableCollection<Mission>(missions);
        }

        public void ImportData(WillowTwoPlayerSaveGame saveGame)
        {
            PlaythroughData = new List<MissionPlaythroughData>();
            for (int i = 0; i < saveGame.MissionPlaythroughs.Count; i++)
            {
                var pt = saveGame.MissionPlaythroughs[i];
                PlaythroughData.Add(new MissionPlaythroughData()
                {
                    PlayThroughNumber = pt.PlayThroughNumber,
                    ActiveMission = pt.ActiveMission,
                    PendingMissionRewards = pt.PendingMissionRewards,
                    FilteredMissions = pt.FilteredMissions
                });
                if (i == 0)
                {
                    NormalMissions = new ObservableCollection<MissionData>(pt.MissionData);
                }
                else if (i == 1)
                {
                    TvhmMissions = new ObservableCollection<MissionData>(pt.MissionData);
                }
                else if (i == 2)
                {
                    UvhmMissions = new ObservableCollection<MissionData>(pt.MissionData);
                }
            }
            ShowTotal();
        }

        public void ExportData(WillowTwoPlayerSaveGame saveGame)
        {
            saveGame.MissionPlaythroughs.Clear();
            for (int i = 0; i < PlaythroughData.Count; i++)
            {
                var pt = PlaythroughData[i];
                var newPt = new MissionPlaythroughData()
                {
                    PlayThroughNumber = pt.PlayThroughNumber,
                    ActiveMission = pt.ActiveMission,
                    PendingMissionRewards = pt.PendingMissionRewards,
                    FilteredMissions = pt.FilteredMissions
                };
                if (i == 0)
                {
                    newPt.MissionData = NormalMissions.ToList();
                }
                else if (i == 1)
                {
                    newPt.MissionData = TvhmMissions.ToList();
                }
                else if (i == 2)
                {
                    newPt.MissionData = UvhmMissions.ToList();
                }
                saveGame.MissionPlaythroughs.Add(newPt);
            }
        }
    }

    internal enum PlaythroughKind
    {
        Normal,
        TVHM,
        UVHM
    }

    internal class StoryPlaythrough
    {
        public StoryPlaythrough(PlaythroughKind kind)
        {
            Kind = kind;
        }

        public PlaythroughKind Kind { get; }

        public int Index
        {
            get
            {
                if (Kind == PlaythroughKind.TVHM)
                {
                    return 1;
                }
                if (Kind == PlaythroughKind.UVHM)
                {
                    return 2;
                }
                return 0;
            }
        }

        public string Title
        {
            get
            {
                if (Kind == PlaythroughKind.TVHM)
                {
                    return "True Vault Hunter Mode";
                }
                if (Kind == PlaythroughKind.UVHM)
                {
                    return "Ultimate Vault Hunter Mode";
                }
                return "Normal Mode";
            }
        }
    }
}
