using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AgentProto
{
    public delegate void StateEvent(ProtoParty sender, ProtoState state);

    public delegate void StateErrorEvent(ProtoParty sender, ProtoState state, Exception ex);

    public abstract class ProtoParty
    {
        public StateEvent OnRequest { get; set; }

        public StateEvent OnResponse { get; set; }

        public StateErrorEvent OnAbort { get; set; }

        protected Config Config;

        protected IFs Fs;

        protected readonly ManualResetEvent AllDone =
            new ManualResetEvent(false);

        public ProtoParty(Config config, IFs fs)
        {
            Config = config;
            Fs = fs;
        }

        public virtual void Abort(ProtoState state, Exception e)
        {
            state?.Abort(e);
            AllDone.Set();
            OnAbort?.Invoke(this, state, e);
        }

        public virtual void Complete(ProtoState state)
        {
            state.Complete();
            OnResponse?.Invoke(this, state);
        }
    }
}
