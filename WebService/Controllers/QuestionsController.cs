﻿using AutoMapper;
using DatabaseService;
using DatabaseService.Modules;
using DatabaseService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace WebService.Controllers
{
    [ApiController]
    [Route("api/questions")]
    [Authorize]
    public class QuestionsController : ControllerBase
    {
        private ISearchDataService _dataService;
        private ISharedService _sharedService;
        private IAnnotationService _annotationService;
        private IHistoryService _historyService;
        private IMapper _mapper;

        public QuestionsController(
            ISearchDataService dataService,
            ISharedService sharedService,
            IAnnotationService annotationService,
            IHistoryService historyService,
            IMapper mapper)
        {
            _dataService = dataService;
            _sharedService = sharedService;
            _mapper = mapper;
            _historyService = historyService;
            _annotationService = annotationService;

        }

        [HttpGet(Name = nameof(BrowseQuestions))]
        //examples http://localhost:5001/api/questions
        // http://localhost:5001/api/questions?page=10&pageSize=5
        public ActionResult BrowseQuestions([FromQuery] PagingAttributes pagingAttributes)
        {
            var categories = _dataService.GetQuestions(pagingAttributes);

            var result = CreateResult(categories, pagingAttributes);

            return Ok(result);
        }
/*
       [HttpGet("{questionId}", Name = nameof(GetQuestion))]
        //example http://localhost:5001/api/questions/19
        public ActionResult GetQuestion(int questionId)
        {
            var question = _dataService.GetQuestion(questionId);
            if (question == null)
            {
                return NotFound();
            }
            return Ok(CreateQuestionDto(question));
        }
*/
        //[Route("thread/{questionId}/{postId?}")]
        [HttpGet("thread/{questionId}/{postId?}", Name = nameof(GetThread))]
        //example http://localhost:5001/api/questions/thread/19
        //get the whole thread of question+asnswers
        public ActionResult GetThread(int questionId, int? postId)
        {
            bool useridok = false;
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            int userId;
            if (Int32.TryParse(claimsIdentity.FindFirst(ClaimTypes.Name)?.Value, out userId))
            {
                useridok = true; //becomes true when we get an int in userId
            }

           // if (questionId > 0) //dont know proper way to do this
          //  {
                var t = _sharedService.GetThread(questionId);
                if (t != null)
                {
                    if (useridok) //then valid user made the request
                    {
                        ///call to add browse history here
                        History browsehist = new History();
                        browsehist.Userid = userId;
                        if (postId != null)
                        {
                            browsehist.Postid = (int)postId;
                        }
                        else browsehist.Postid = questionId;
                        _historyService.Add(browsehist);
                    }

                    List<PostsThreadDto> thread = new List<PostsThreadDto>();
                    //createthreaddto
                    foreach (Posts p in t)
                    {
                        PostsThreadDto pt = new PostsThreadDto();
                        pt.Id = p.Id;
                        pt.Parentid = p.Parentid;
                        pt.Title = p.Title;
                        pt.Body = p.Body;

                    PagingAttributes pagingAttributes = new PagingAttributes();
                    List<AnnotationsMinimalDto> finalanno = new List<AnnotationsMinimalDto>();
                    List<AnnotationsDto> tempanno = new List<AnnotationsDto>();
                        tempanno = _annotationService.GetAnnotationsWithPostId(userId, p.Id, pagingAttributes);
                    foreach (AnnotationsDto ta in tempanno)
                    {
                        AnnotationsMinimalDto fa = new AnnotationsMinimalDto();
                        fa.Body = ta.Body;
                        fa.Date = ta.Date;
                        finalanno.Add(fa);
                    }
                    pt.Annotations = finalanno;
                    // pt.createBookamrkLink = Url.Link(  nameof(),  new { questionId = question.Id });
                    AnnotationsDto anno = new AnnotationsDto();
                        anno.Body = "form/similar would be here to POST a new annotation";
                        anno.PostId = p.Id;
                        pt.createAnnotationLink = Url.Link(nameof(AnnotationsController.AddAnnotation), anno); 
                    // i know its supposed to be a form/post. just thought it'd be neat to have a link mockup. oh well maybe its more confusing this way :(
                        thread.Add(pt);
                    }

                    return Ok(thread);
                } else return NotFound();
           // }
           // else
           // {
           //     return NotFound();
           // }

        }


               ///////////////////
               //
               // Helpers
               //
               //////////////////////

        private QuestionDto CreateQuestionDto(Questions question)
        {

            //var dto = _mapper.Map<QuestionDto>(question);
            var dto = new QuestionDto();
            dto.Link = Url.Link(
                    nameof(GetThread),
                    new { questionId = question.Id });
            dto.Id = question.Id;
            dto.Title = question.Title;
            dto.Body = question.Body;
            return dto;
        }
        
        private object CreateResult(IEnumerable<Questions> questions, PagingAttributes attr)
        {
            var totalItems = _sharedService.NumberOfQuestions();
            var numberOfPages = Math.Ceiling((double)totalItems / attr.PageSize);

            var prev = attr.Page > 1
                ? CreatePagingLink(attr.Page - 1, attr.PageSize)
                : null;
            var next = attr.Page < numberOfPages - 1
                ? CreatePagingLink(attr.Page + 1, attr.PageSize)
                : null;

            return new
            {
                totalItems,
                numberOfPages,
                prev,
                next,
                items = questions.Select(CreateQuestionDto)
                //items = questions
            };
        }

        private string CreatePagingLink(int page, int pageSize)
        {
            return Url.Link(nameof(BrowseQuestions), new { page, pageSize });
        } 
    }
}
