using Microsoft.AspNetCore.SignalR;

namespace Turnero.Hubs;

/// Canal de comunicacion en tiempo real entre el servidor y la pantalla.
/// No necesita metodos propios: el servidor empuja los avisos
/// y la pantalla solo escucha.
public class TurneroHub : Hub
{
}