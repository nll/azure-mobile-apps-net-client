using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Query;
using Microsoft.WindowsAzure.MobileServices.Table.Sync;

namespace Microsoft.WindowsAzure.MobileServices.Sync
{
    internal class SplitPullAction : TableAction
    {
        private readonly IDictionary<string, string> parameters;
        private readonly MobileServiceRemoteTableOptions options;
        private readonly PullOptions pullOptions;
        private readonly MobileServiceObjectReader reader;

        private readonly SplitPullOptions splitPullOptions;
        
        private Task pendingAction;

        public SplitPullAction(
                                MobileServiceTable table,
                                MobileServiceTableKind tableKind,
                                MobileServiceSyncContext context,
                                string queryId,
                                MobileServiceTableQueryDescription query,
                                IDictionary<string, string> parameters,
                                IEnumerable<string> relatedTables,
                                OperationQueue operationQueue,
                                MobileServiceSyncSettingsManager settings,
                                IMobileServiceLocalStore store,
                                MobileServiceRemoteTableOptions options,
                                PullOptions pullOptions,
                                MobileServiceObjectReader reader,
                                SplitPullOptions splitPullOptions,
                                CancellationToken cancellationToken) 
            : base(table, tableKind, queryId, query, relatedTables, context, operationQueue, settings, store, cancellationToken)
        {
            this.parameters = parameters;
            this.options = options;
            this.pullOptions = pullOptions;
            this.reader = reader;
            this.splitPullOptions = splitPullOptions;
        }

        protected override Task<bool> HandleDirtyTable()
        {
            // there are pending operations on the same table so defer the action
            this.pendingAction = this.Context.DeferTableActionAsync(this);
            // we need to return in order to give PushAsync a chance to execute so we don't await the pending push
            return Task.FromResult(false);
        }

        protected override Task WaitPendingAction()
        {
            return this.pendingAction ?? Task.FromResult(0);
        }

        protected internal override Task ProcessTableAsync()
        {
            var actions = splitPullOptions.FieldValues.Select(GeneratePullAction);
            return Task.WhenAll(actions.Select(it => it.ProcessTableAsync()));
        }

        private PullAction GeneratePullAction(string value)
        {
            var query = Query.Clone();

            var fieldNode = new MemberAccessNode(null, splitPullOptions.FieldName);
            var valueNode = new ConstantNode(value);
            var equalNode = new BinaryOperatorNode(BinaryOperatorKind.Equal, fieldNode, valueNode);
            if (query.Filter == null)
            {
                query.Filter = equalNode;
            }
            else
            {
                var originalFilterAndGreaterThanDeltaNode = new BinaryOperatorNode(BinaryOperatorKind.And, query.Filter, equalNode);
                query.Filter = originalFilterAndGreaterThanDeltaNode;
            }

            if (string.IsNullOrEmpty(QueryId))
            {
                throw new NotSupportedException("It needs to be a incremental query");
            }

            var queryId = $"{QueryId}_{splitPullOptions.QueryFieldPrefix}_{value}";
            
            return new PullAction(Table, TableKind, Context, queryId, query, parameters, RelatedTables, OperationQueue, Settings, Store, options, pullOptions, reader, CancellationToken);
        }
    }
}