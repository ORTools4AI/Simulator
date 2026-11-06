// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.core.engine
{
    public class ParameterManager : ModelParameterManager, ExperimentParameterManager
    {
        private SortedDictionary<string, Parameter> _parameters;

        public ParameterManager()
        {
            _parameters = new SortedDictionary<string, Parameter>(StringComparer.OrdinalIgnoreCase);
        }

        public void declareExperimentParameter(Type type, string name)
        {
            _parameters[name] = Parameter.createExperimentParameter(type, name);
        }

        public void declareExperimentParameter(Type type, string name, object defaultValue)
        {
            _parameters[name] = Parameter.createExperimentParameter(type, name, defaultValue);
        }

        public void declareModelParameter(Type type, string name)
        {
            _parameters[name] = Parameter.createModelParameter(type, name);
        }

        public void initializeModelParameter(Type type, string name, object value)
        {
            _parameters[name] = Parameter.createModelParameter(type, name, value);
        }

        public void assignModelParameter(string name, object value)
        {
            if (_parameters.TryGetValue(name, out Parameter param)
                && param.getParameterType() == Parameter.ParameterType.MODELPARAMETER)
            {
                param.setValue(value);
            }
            //TODO: else (Exception) param not found or wrong type
        }

        public void assignExperimentParameter(string name, object value)
        {
            if (_parameters.TryGetValue(name, out Parameter param)
                && param.getParameterType() == Parameter.ParameterType.EXPERIMENTPARAMETER)
            {
                param.setValue(value);
            }
            //TODO: else (Exception) param not found or wrong type
        }

        public object getParameterValue(string name)
        {
            if (_parameters.TryGetValue(name, out Parameter param))
                return param.getValue();
            //TODO: (Exception) param + name + not declared
            return null;
        }

        public ICollection<Parameter> getParameters()
        {
            return _parameters.Values;
        }
    }
}
