namespace HackF5.UnitySpy.Gui.Mvvm
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Nito.AsyncEx;

    [Register(RegistrationType.Singleton)]
    public class OperationController
    {
        private readonly BlockingCollection<object> block = new BlockingCollection<object>(1);

        private readonly object tokenItem = new object();

        public OperationController()
        {
            this.block.Add(this.tokenItem);
        }

        public bool IsExecuting => this.block.Count == 0;

        public bool TryRent(out IDisposable token)
        {
            if (!this.block.TryTake(out _))
            {
                token = default;
                return false;
            }

            token = new OperationControllerToken(this);
            return true;
        }

        public IDisposable Rent()
        {
            this.block.Take();
            return new OperationControllerToken(this);
        }

        private struct OperationControllerToken : IDisposable
        {
            private readonly OperationController controller;

            public OperationControllerToken(OperationController controller)
            {
                this.controller = controller;
            }

            public void Dispose()
            {
                this.controller.block.Add(this.controller.tokenItem);
            }
        }
    }
}