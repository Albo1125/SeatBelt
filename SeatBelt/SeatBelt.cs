using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeatBelt
{
    public class SeatBelt : BaseScript
    {
        private bool seatbeltWarningGiven = false;
        private bool seatbeltOn = false;
        private bool seatbeltGrace = true;
        private bool resetSeatbeltOutOfVeh = true;
        public static Dictionary<int, bool> SeatbeltToggles = new Dictionary<int, bool>();
        private DateTime lastSpottedDateTime = DateTime.MinValue;
        private DateTime lastPlayedChime = DateTime.MinValue;
        private List<Player> spottedPlayers = new List<Player>();

        public SeatBelt()
        {
            EventHandlers["SeatBelt:SyncToggles"] += new Action<dynamic>((dynamic seatbelts) =>
            {
                SeatbeltToggles.Clear();
                if (seatbelts != null)
                {
                    foreach (KeyValuePair<string, object> item in seatbelts)
                    {
                        SeatbeltToggles[Int32.Parse(item.Key)] = bool.Parse(item.Value.ToString());
                    }
                }
            });
            Main();
        }

        private static float ConvertMPHToMPS(float mph)
        {
            return mph / 2.2f;
        }
        private bool LastInputWasController()
        {
            return !CitizenFX.Core.Native.Function.Call<bool>(CitizenFX.Core.Native.Hash._IS_INPUT_DISABLED, 2);
        }

        private async Task Main()
        {
            Notification noti = null;
            while (true)
            {
                await Delay(0);
                if (LocalPlayer != null && LocalPlayer.Character != null)
                {
                    if (LocalPlayer.Character.CurrentVehicle != null && LocalPlayer.Character.CurrentVehicle.Exists() 
                        && !LocalPlayer.Character.CurrentVehicle.Model.IsBike && !LocalPlayer.Character.CurrentVehicle.Model.IsBicycle)
                    {
                        resetSeatbeltOutOfVeh = false;
                        if (!SeatbeltToggles.ContainsKey(LocalPlayer.ServerId))
                        {
                            TriggerServerEvent("SeatBelt:UpdateToggle", LocalPlayer.ServerId, seatbeltGrace || seatbeltOn);
                            SeatbeltToggles[LocalPlayer.ServerId] = seatbeltGrace || seatbeltOn;
                        }
                        if (LocalPlayer.Character.CurrentVehicle.Speed > ConvertMPHToMPS(10))
                        {
                            if (seatbeltGrace)
                            {
                                seatbeltGrace = false;
                                TriggerServerEvent("SeatBelt:UpdateToggle", LocalPlayer.ServerId, seatbeltOn);
                            }
                            if (!seatbeltOn && !seatbeltWarningGiven)
                            {
                                seatbeltWarningGiven = true;
                                TriggerServerEvent("SeatBelt:Warning", LocalPlayer.ServerId);
                            }

                            if (!seatbeltOn && !seatbeltGrace && seatbeltWarningGiven && (DateTime.Now - lastPlayedChime).Seconds >= 5)
                            {
                                TriggerEvent("InteractSound_CL:PlayOnOne", "SeatbeltChime", 0.9f);
                                lastPlayedChime = DateTime.Now;
                            }
                        }
                        
                        if (seatbeltOn)
                        {
                            Game.DisableControlThisFrame(0, Control.VehicleExit);
                            if (Game.IsDisabledControlJustPressed(0, Control.VehicleExit))
                            {
                                //noti = Screen.ShowNotification("Seat belt warning: toggle your seat belt using Ctrl S.");
                                TriggerServerEvent("SeatBelt:Warning", LocalPlayer.ServerId);
                            }
                        }

                        if ((Game.IsDisabledControlPressed(0, Control.VehicleSubDescend) || Game.IsControlPressed(0, Control.VehicleSubDescend)) && (Game.IsControlJustPressed(0, Control.MoveDownOnly) || Game.IsDisabledControlJustPressed(0, Control.MoveDownOnly))
                            && !LastInputWasController())
                        {
                            seatbeltOn = !seatbeltOn;
                            seatbeltGrace = false;
                            TriggerServerEvent("SeatBelt:UpdateToggle", LocalPlayer.ServerId, seatbeltOn);
                            Screen.ShowSubtitle("Seat belt " + (seatbeltOn ? "fastened." : "unfastened."));
                            if (noti != null)
                            {
                                noti.Hide();
                            }
                        }

                    }
                    else
                    {
                        seatbeltWarningGiven = false;
                        seatbeltGrace = true;
                        seatbeltOn = false;
                        if (!resetSeatbeltOutOfVeh)
                        {
                            resetSeatbeltOutOfVeh = true;
                            TriggerServerEvent("SeatBelt:UpdateToggle", LocalPlayer.ServerId, true);
                        }
                    }

                    if (spottedPlayers.Count > 0 && (DateTime.Now - lastSpottedDateTime).Seconds > 11)
                    {
                        spottedPlayers.Clear();
                        lastSpottedDateTime = DateTime.MinValue;
                    }

                    foreach (Player p in Players)
                    {
                        if (p != null && !spottedPlayers.Contains(p) && p != LocalPlayer && p.Character != null && p.Character.Exists() && Vector3.Distance(LocalPlayer.Character.Position, p.Character.Position) < 19 && p.Character.IsInVehicle() 
                                && p.Character.CurrentVehicle != null && p.Character.CurrentVehicle.IsEngineRunning)
                        {
                            if (!p.Character.CurrentVehicle.Model.IsBike && !p.Character.CurrentVehicle.Model.IsBicycle && SeatbeltToggles.ContainsKey(p.ServerId) && !SeatbeltToggles[p.ServerId])
                            {
                                string desc = "passenger";
                                if (p.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == p.Character)
                                {
                                    desc = "driver";
                                }
                                else if (p.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Passenger) == p.Character)
                                {
                                    desc = "front seat passenger";
                                }
                                else if (p.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.LeftRear) == p.Character)
                                {
                                    desc = "back left passenger";
                                }
                                else if (p.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.RightRear) == p.Character)
                                {
                                    desc = "back right passenger";
                                }
                                string modelName = API.GetDisplayNameFromVehicleModel((uint)API.GetEntityModel(p.Character.CurrentVehicle.Handle));
                                string plate = API.GetVehicleNumberPlateText(p.Character.CurrentVehicle.Handle).Trim();
                                Screen.ShowNotification("No seat belt: " + desc + " of the " + modelName + ".");
                                TriggerEvent("chatMessage", "Observations", new int[] { 255, 255, 0 }, "You notice the " + desc + " of the " + modelName + " (" + plate + ") isn't wearing a seat belt.");
                                lastSpottedDateTime = DateTime.Now;
                                spottedPlayers.Add(p);
                            }
                        }
                    }
                }
            }
        }
    }
}
