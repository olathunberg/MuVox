using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using RecordToMP3.Features.Processor;
using RecordToMP3.Features.Recorder;

namespace RecordToMP3
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            //if (ViewModelBase.IsInDesignModeStatic)
            //{
            //    SimpleIoc.Default.Register<IDataService, Design.DesignDataService>();
            //}
            //else
            //{
            //    SimpleIoc.Default.Register<IDataService, DataService>();
            //}

            SimpleIoc.Default.Register<RecorderViewModel>();

            SimpleIoc.Default.Register<ProcessorViewModel>();
        }

        public RecorderViewModel Recorder
        {
            get { return SimpleIoc.Default.GetInstance<RecorderViewModel>(); }
        }
  
        public ProcessorViewModel Processor
        {
            get { return SimpleIoc.Default.GetInstance<ProcessorViewModel>(); }
        }
    }
}
