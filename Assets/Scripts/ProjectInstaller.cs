using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        this.Container.Bind<IFileService>().To<FileService>().AsTransient();
        this.Container.Bind<IDirectoryService>().To<DirectoryService>().AsTransient();
        this.Container.Bind<IEncodingService>().To<EncodingService>().AsTransient();
        this.Container.Bind<IAuthenticationService>().To<AppAuthentication>().AsSingle();
        this.Container.Bind<IUserProfileManager>().To<UserProfileManager>().AsSingle();
    }
}