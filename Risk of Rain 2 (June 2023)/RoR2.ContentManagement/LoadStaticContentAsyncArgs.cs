using System;
using HG;

namespace RoR2.ContentManagement;

public class LoadStaticContentAsyncArgs
{
	private readonly IProgress<float> progressReceiver;

	public readonly ReadOnlyArray<ContentPackLoadInfo> peerLoadInfos;

	public LoadStaticContentAsyncArgs(IProgress<float> progressReceiver, ReadOnlyArray<ContentPackLoadInfo> peerLoadInfos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		this.progressReceiver = progressReceiver;
		this.peerLoadInfos = peerLoadInfos;
	}

	public void ReportProgress(float progress)
	{
		progressReceiver.Report(progress);
	}
}
