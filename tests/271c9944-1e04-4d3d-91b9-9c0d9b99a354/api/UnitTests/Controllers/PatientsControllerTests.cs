using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NGR.Api.Controllers;
using NGR.Api.DTOs;
using NGR.Api.Services;
using System.Security.Claims;
using Xunit;

namespace NGR.Api.UnitTests.Controllers;

/// <summary>
/// Unit tests for PatientsController
/// </summary>
public class PatientsControllerTests
{
    private readonly Mock<IPatientService> _mockPatientService;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ILogger<PatientsController>> _mockLogger;
    private readonly PatientsController _controller;

    public PatientsControllerTests()
    {
        _mockPatientService = new Mock<IPatientService>();
        _mockAuditService = new Mock<IAuditService>();
        _mockLogger = new Mock<ILogger<PatientsController>>();
        
        _controller = new PatientsController(
            _mockPatientService.Object,
            _mockAuditService.Object,
            _mockLogger.Object);

        // Setup controller context with authenticated user
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "test@example.com"),
            new Claim("sub", "test-user-id")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    #region GetPatients Tests

    [Fact]
    public async Task GetPatients_WithNoFilters_ReturnsOkWithPatients()
    {
        // Arrange
        var patients = new List<PatientDto>
        {
            new PatientDto { Id = 1, FirstName = "John", LastName = "Doe", Status = "Active" },
            new PatientDto { Id = 2, FirstName = "Jane", LastName = "Smith", Status = "Active" }
        };
        _mockPatientService.Setup(s => s.GetPatientsAsync(null, null, null, 1, 50))
            .ReturnsAsync(patients);

        // Act
        var result = await _controller.GetPatients();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPatients = okResult.Value.Should().BeAssignableTo<IEnumerable<PatientDto>>().Subject;
        returnedPatients.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPatients_WithCareProgramFilter_ReturnsFilteredPatients()
    {
        // Arrange
        var careProgramId = 1;
        var patients = new List<PatientDto>
        {
            new PatientDto { Id = 1, FirstName = "John", LastName = "Doe", CareProgramId = careProgramId }
        };
        _mockPatientService.Setup(s => s.GetPatientsAsync(careProgramId, null, null, 1, 50))
            .ReturnsAsync(patients);

        // Act
        var result = await _controller.GetPatients(careProgramId: careProgramId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPatients = okResult.Value.Should().BeAssignableTo<IEnumerable<PatientDto>>().Subject;
        returnedPatients.Should().HaveCount(1);
        returnedPatients.First().CareProgramId.Should().Be(careProgramId);
    }

    [Fact]
    public async Task GetPatients_WithSearchTerm_ReturnsMatchingPatients()
    {
        // Arrange
        var searchTerm = "John";
        var patients = new List<PatientDto>
        {
            new PatientDto { Id = 1, FirstName = "John", LastName = "Doe" }
        };
        _mockPatientService.Setup(s => s.GetPatientsAsync(null, null, searchTerm, 1, 50))
            .ReturnsAsync(patients);

        // Act
        var result = await _controller.GetPatients(searchTerm: searchTerm);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPatients = okResult.Value.Should().BeAssignableTo<IEnumerable<PatientDto>>().Subject;
        returnedPatients.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPatients_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var page = 2;
        var pageSize = 10;
        var patients = new List<PatientDto>
        {
            new PatientDto { Id = 11, FirstName = "Patient", LastName = "Eleven" }
        };
        _mockPatientService.Setup(s => s.GetPatientsAsync(null, null, null, page, pageSize))
            .ReturnsAsync(patients);

        // Act
        var result = await _controller.GetPatients(page: page, pageSize: pageSize);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetPatients_WhenServiceThrows_Returns500()
    {
        // Arrange
        _mockPatientService.Setup(s => s.GetPatientsAsync(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetPatients();

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetPatient Tests

    [Fact]
    public async Task GetPatient_WithValidId_ReturnsOkWithPatient()
    {
        // Arrange
        var patientId = 1;
        var patient = new PatientDto { Id = patientId, FirstName = "John", LastName = "Doe" };
        _mockPatientService.Setup(s => s.GetPatientByIdAsync(patientId))
            .ReturnsAsync(patient);

        // Act
        var result = await _controller.GetPatient(patientId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPatient = okResult.Value.Should().BeOfType<PatientDto>().Subject;
        returnedPatient.Id.Should().Be(patientId);
    }

    [Fact]
    public async Task GetPatient_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var patientId = 999;
        _mockPatientService.Setup(s => s.GetPatientByIdAsync(patientId))
            .ReturnsAsync((PatientDto?)null);

        // Act
        var result = await _controller.GetPatient(patientId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetPatient_WhenServiceThrows_Returns500()
    {
        // Arrange
        _mockPatientService.Setup(s => s.GetPatientByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetPatient(1);

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region CreatePatient Tests

    [Fact]
    public async Task CreatePatient_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreatePatientDto
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            CareProgramId = 1
        };
        var createdPatient = new PatientDto
        {
            Id = 1,
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            DateOfBirth = createDto.DateOfBirth,
            CareProgramId = createDto.CareProgramId
        };
        _mockPatientService.Setup(s => s.CreatePatientAsync(createDto, It.IsAny<string>()))
            .ReturnsAsync(createdPatient);

        // Act
        var result = await _controller.CreatePatient(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(PatientsController.GetPatient));
        var returnedPatient = createdResult.Value.Should().BeOfType<PatientDto>().Subject;
        returnedPatient.Id.Should().Be(1);
        
        // Verify audit log was called
        _mockAuditService.Verify(s => s.LogActionAsync(
            "Patient",
            "1",
            "Create",
            null,
            It.IsAny<object>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreatePatient_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreatePatientDto
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            CareProgramId = 999
        };
        _mockPatientService.Setup(s => s.CreatePatientAsync(createDto, It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("Care program not found"));

        // Act
        var result = await _controller.CreatePatient(createDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreatePatient_WhenServiceThrows_Returns500()
    {
        // Arrange
        var createDto = new CreatePatientDto
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            CareProgramId = 1
        };
        _mockPatientService.Setup(s => s.CreatePatientAsync(createDto, It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CreatePatient(createDto);

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region UpdatePatient Tests

    [Fact]
    public async Task UpdatePatient_WithValidData_ReturnsOkWithUpdatedPatient()
    {
        // Arrange
        var patientId = 1;
        var updateDto = new UpdatePatientDto
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Status = "Active"
        };
        var existingPatient = new PatientDto { Id = patientId, FirstName = "Old", LastName = "Name" };
        var updatedPatient = new PatientDto
        {
            Id = patientId,
            FirstName = updateDto.FirstName,
            LastName = updateDto.LastName,
            DateOfBirth = updateDto.DateOfBirth,
            Status = updateDto.Status
        };
        
        _mockPatientService.Setup(s => s.GetPatientByIdAsync(patientId))
            .ReturnsAsync(existingPatient);
        _mockPatientService.Setup(s => s.UpdatePatientAsync(patientId, updateDto, It.IsAny<string>()))
            .ReturnsAsync(updatedPatient);

        // Act
        var result = await _controller.UpdatePatient(patientId, updateDto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedPatient = okResult.Value.Should().BeOfType<PatientDto>().Subject;
        returnedPatient.FirstName.Should().Be(updateDto.FirstName);
        
        // Verify audit log was called
        _mockAuditService.Verify(s => s.LogActionAsync(
            "Patient",
            patientId.ToString(),
            "Update",
            It.IsAny<object>(),
            It.IsAny<object>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePatient_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var patientId = 999;
        var updateDto = new UpdatePatientDto
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Status = "Active"
        };
        _mockPatientService.Setup(s => s.GetPatientByIdAsync(patientId))
            .ReturnsAsync((PatientDto?)null);

        // Act
        var result = await _controller.UpdatePatient(patientId, updateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdatePatient_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var patientId = 1;
        var updateDto = new UpdatePatientDto
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Status = "Active"
        };
        var existingPatient = new PatientDto { Id = patientId };
        
        _mockPatientService.Setup(s => s.GetPatientByIdAsync(patientId))
            .ReturnsAsync(existingPatient);
        _mockPatientService.Setup(s => s.UpdatePatientAsync(patientId, updateDto, It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("Invalid data"));

        // Act
        var result = await _controller.UpdatePatient(patientId, updateDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region DeletePatient Tests

    [Fact]
    public async Task DeletePatient_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var patientId = 1;
        var existingPatient = new PatientDto { Id = patientId, FirstName = "John", LastName = "Doe" };
        
        _mockPatientService.Setup(s => s.GetPatientByIdAsync(patientId))
            .ReturnsAsync(existingPatient);
        _mockPatientService.Setup(s => s.DeletePatientAsync(patientId, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeletePatient(patientId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        
        // Verify audit log was called
        _mockAuditService.Verify(s => s.LogActionAsync(
            "Patient",
            patientId.ToString(),
            "Delete",
            It.IsAny<object>(),
            null,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeletePatient_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var patientId = 999;
        _mockPatientService.Setup(s => s.GetPatientByIdAsync(patientId))
            .ReturnsAsync((PatientDto?)null);

        // Act
        var result = await _controller.DeletePatient(patientId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeletePatient_WhenServiceThrows_Returns500()
    {
        // Arrange
        var patientId = 1;
        var existingPatient = new PatientDto { Id = patientId };
        
        _mockPatientService.Setup(s => s.GetPatientByIdAsync(patientId))
            .ReturnsAsync(existingPatient);
        _mockPatientService.Setup(s => s.DeletePatientAsync(patientId, It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.DeletePatient(patientId);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetPatientCount Tests

    [Fact]
    public async Task GetPatientCount_WithNoFilter_ReturnsCount()
    {
        // Arrange
        var expectedCount = 100;
        _mockPatientService.Setup(s => s.GetPatientCountAsync(null))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _controller.GetPatientCount();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var count = okResult.Value.Should().BeOfType<int>().Subject;
        count.Should().Be(expectedCount);
    }

    [Fact]
    public async Task GetPatientCount_WithCareProgramFilter_ReturnsFilteredCount()
    {
        // Arrange
        var careProgramId = 1;
        var expectedCount = 25;
        _mockPatientService.Setup(s => s.GetPatientCountAsync(careProgramId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _controller.GetPatientCount(careProgramId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var count = okResult.Value.Should().BeOfType<int>().Subject;
        count.Should().Be(expectedCount);
    }

    #endregion
}
