﻿using UseCase = FC.Codeflix.Catalog.Application.UseCases.Video.DeleteVideo;
using DomainEntity = FC.Codeflix.Catalog.Domain.Entity;
using FluentAssertions;
using Xunit;
using FC.Codeflix.Catalog.Domain.Repository;
using FC.Codeflix.Catalog.Application.Interfaces;
using Moq;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FC.Codeflix.Catalog.UnitTests.Application.Video.DeleteVideo;

[Collection(nameof(DeleteVideoTestFixture))]
public class DeleteVideoTest
{
    private readonly DeleteVideoTestFixture _fixture;
    private readonly UseCase.DeleteVideo _useCase;
    private readonly Mock<IVideoRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IStorageService> _storageService;

    public DeleteVideoTest(DeleteVideoTestFixture fixture)
    {
        _fixture = fixture;
        _repositoryMock = new Mock<IVideoRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _useCase = new UseCase.DeleteVideo(
            _repositoryMock.Object, 
            _unitOfWorkMock.Object,
            _storageService.Object
        );
    }

    [Fact(DisplayName = nameof(DeleteVideo))]
    [Trait("Application", "DeleteVideo - Use Cases")]
    public async Task DeleteVideo()
    {
        var videoExample = _fixture.GetValidVideo();
        var input = _fixture.GetValidInput(videoExample.Id);
        _repositoryMock.Setup(x => x.Get(
                It.Is<Guid>(id => id == videoExample.Id),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(videoExample);

        await _useCase.Handle(input, CancellationToken.None);

        _repositoryMock.VerifyAll();
        _repositoryMock.Verify(x => x.Delete(
                It.Is<DomainEntity.Video>(video => video.Id == videoExample.Id),
                It.IsAny<CancellationToken>())
            , Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()));
    }

    [Fact(DisplayName = nameof(DeleteVideo))]
    [Trait("Application", "DeleteVideo - Use Cases")]
    public async Task DeleteVideoWithAllMediasAndClearStorage()
    {
        var videoExample = _fixture.GetValidVideo();
        videoExample.UpdateMedia(_fixture.GetValidMediaPath());
        videoExample.UpdateTrailer(_fixture.GetValidMediaPath());
        var filePaths = new List<string>() { 
            videoExample.Media!.FilePath, 
            videoExample.Trailer!.FilePath 
        };
        var input = _fixture.GetValidInput(videoExample.Id);
        _repositoryMock.Setup(x => x.Get(
                It.Is<Guid>(id => id == videoExample.Id),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(videoExample);

        await _useCase.Handle(input, CancellationToken.None);

        _repositoryMock.VerifyAll();
        _repositoryMock.Verify(x => x.Delete(
                It.Is<DomainEntity.Video>(video => video.Id == videoExample.Id),
                It.IsAny<CancellationToken>())
            , Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()));
        _storageService.Verify(x => x.Delete(
                It.Is<string>(filePath => filePaths.Contains(filePath)),
                It.IsAny<CancellationToken>())
            , Times.Exactly(2));
        _storageService.Verify(x => x.Delete(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>())
            , Times.Exactly(2));
    }
}
