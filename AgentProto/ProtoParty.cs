using System;
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

        protected ProtoParty(Config config, IFs fs)
        {
            Config = config;
            Fs = fs;
        }

        public virtual void Abort(ProtoState state, Exception e)
        {
            state?.Abort(e);
            AllDone.Reset();
            OnAbort?.Invoke(this, state, e);
        }

        public virtual object Complete(ProtoState state)
        {
            var res = state.Complete();
            OnResponse?.Invoke(this, state);
            AllDone.Reset();
            return res;
        }
    }
}
