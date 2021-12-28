local seatbelts = {}

RegisterServerEvent("SeatBelt:UpdateToggle")
AddEventHandler('SeatBelt:UpdateToggle', function(playerid, toggle)
	seatbelts[tostring(playerid)] = tostring(toggle)
	TriggerClientEvent("SeatBelt:SyncToggles", -1, seatbelts)
end)

RegisterServerEvent("SeatBelt:Warning")
AddEventHandler('SeatBelt:Warning', function(netid)
	TriggerClientEvent("pNotify:SendNotification", netid, {text = "Seat belt warning: toggle your seat belt using Ctrl S.", type = "warning", timeout = 3500, layout = "centerRight"})
end)

function seatbeltsync()
	TriggerClientEvent("SeatBelt:SyncToggles", -1, seatbelts)
	SetTimeout(40000, seatbeltsync)
end
SetTimeout(1000, seatbeltsync)