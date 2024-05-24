using Application.DTO;
using Domain.IRepository;
using Domain.Model;
using Gateway;

namespace Application.Services;

public class ProjectService
{
    private readonly IProjectRepository _projectRepository;

    // private readonly ProjectGateway _projectGateway;
    private readonly IMessagePublisher _projectGateway;
    // private readonly ProjectGatewayUpdate _projectGatewayUpdate;
    private readonly IMessageUpdatePublisher _projectGatewayUpdate;

    public ProjectService(IProjectRepository projectRepository, IMessagePublisher projectGateway, IMessageUpdatePublisher projectGatewayUpdate)
    {
        _projectRepository = projectRepository;
        _projectGateway = projectGateway;
        _projectGatewayUpdate = projectGatewayUpdate;
    }

    public async Task<IEnumerable<ProjectDTO>> GetAll()
    {
        IEnumerable<Project> projects = await _projectRepository.GetProjectsAsync();

        if (projects is not null)
        {
            IEnumerable<ProjectDTO> projectsDTO = ProjectDTO.ToDTO(projects);
            return projectsDTO;
        }

        return null;
    }

    public async Task<ProjectDTO> GetById(long id)
    {
        Project project = await _projectRepository.GetProjectByIdAsync(id);

        if (project is not null)
        {
            ProjectDTO projectDTO = ProjectDTO.ToDTO(project);
            return projectDTO;
        }

        return null;
    }

    public async Task<ProjectDTO> AddFromAMQP(ProjectDTO projectDTO, List<string> errorMessages)
    {
        bool pExists = await _projectRepository.ProjectExists(projectDTO.Id, projectDTO.Name);
        if (pExists)
        {
            errorMessages.Add("Project already exists.");
            return null;
        }

        try
        {
            Project project = ProjectDTO.ToDomain(projectDTO);
            Project projectSaved = await _projectRepository.Add(project);
            ProjectDTO projectDTOSaved = ProjectDTO.ToDTO(projectSaved);

            return projectDTOSaved;
        }
        catch (ArgumentException ex)
        {
            errorMessages.Add(ex.Message);
            return null;
        }
    }

    public async Task<ProjectDTO> AddFromRest(ProjectDTO projectDTO, List<string> errorMessages)
        {
            ProjectDTO projectDTOSaved = await AddFromAMQP(projectDTO, errorMessages);

            if (projectDTOSaved is not null)
            {
                string jsonMessage = ProjectGatewayDTO.Serialize(projectDTOSaved);
                _projectGateway.publish(jsonMessage);
            }
            return projectDTOSaved;
        }

    public async Task<bool> Update(long id, ProjectDTO projectDTO, List<string> errorMessages, bool cameFromSwagger)
    {
        Project project = await _projectRepository.GetProjectByIdAsync(id);

        if (project is not null)
        {
            ProjectDTO.UpdateToDomain(project, projectDTO);
            await _projectRepository.Update(project, errorMessages);

            if (cameFromSwagger)
            {
                string jsonMessage = ProjectGatewayDTO.Serialize(projectDTO);
                _projectGatewayUpdate.publish(jsonMessage);
            }

            return true;
        }
        else
        {
            errorMessages.Add("Not found");
            return false;
        }
    }

}
