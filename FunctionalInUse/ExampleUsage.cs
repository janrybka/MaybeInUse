using System;

namespace FunctionalInUse
{
    public class ExampleUsage
    {
        private Result<Candidate> CreateCandidate(BulkUploadCandidate uploadCandidate, BulkUploadCandidateGuardian candidateGuardian)
        {
            Contracts.Require(uploadCandidate != null);
            Contracts.Require(candidateGuardian != null);
            var uc = uploadCandidate;

            var fullNameOrError = FullName.Create(uc.FirstName, uc.LastName);
            var dobOrError = DateOfBirth.Create(uc.DateOfBirth);
            var candidateNumberOrError = CandidateNumber.CreateOptional(uc.CandidateNumber);
            var phoneNumberOrError = PhoneNumber.CreateOptional(uc.MobilePhoneNumber);

            return Result.Combine(fullNameOrError, dobOrError, candidateNumberOrError, phoneNumberOrError)
                .OnSuccess(() => Candidate.Create(fullNameOrError.Value, dobOrError.Value, uc.Gender))
                .OnSuccess(x => x.SetCandidateNumber(candidateNumberOrError.Value))
                .OnSuccess(x => x.SetUciNumber(uc.UCI))
                .OnSuccess(x => x.SetPhoneNumber(phoneNumberOrError.Value));
        }

        private Result<CandidateExamsRegistrations> CreateCandidateExamRegistrations(
        List<ExamOptionPriceListEntry> allowedExamOptions,
        CandidateExamsRegistrationsCollection allRegistrations,
        ExamRegistrationRow row,
        SessionInfo session,
        PreparationCentreInfo preparationCentreInfo,
        SessionChronology sessionChronology
        )
        {
            var candidateResult = CreateCandidate(row.Candidate, row.CandidateGuardian);


            var identifierResult = PersonIdentifier.Create(row.Candidate.IdType, row.Candidate.IdNumber, row.Candidate.IdExpiryDate);

            var registrationsResults = new List<Result<Registration>>();
            foreach (var eo in row.ExamOptions)
            {
                var registrationTypeValue = RegistrationTypeValueExtensions.Cast(eo.RegisteringTypeCellValue);

                var combinedExamOptionResult = Result.Ok();
                var domainExamOptionResult = GetExamOptionIfAllowed(allowedExamOptions, eo);

                ExamOption examOption = null;
                if (domainExamOptionResult.IsSuccess)
                {
                    var examOptionResult = ExamOption.Create(domainExamOptionResult.Value.Code, domainExamOptionResult.Value.SyllabusOption, domainExamOptionResult.Value.SyllabusId);
                    if (examOptionResult.IsSuccess)
                    {
                        examOption = examOptionResult.Value;
                    }
                    combinedExamOptionResult = Result.Combine(domainExamOptionResult, examOptionResult);
                }

                var previousSession = sessionChronology.GetByName(row.PreviousSession);
                int? prevSessionId = previousSession.HasValue ? previousSession.Value.Id : default(int?);
                var regTypeOrError = RegistrationType.Create(
                    registrationTypeValue, row.GroupAward, row.PreviousCandidateNumber, row.PreviousCentreNumber, prevSessionId);

                var registrationsError = Result.Combine(combinedExamOptionResult, regTypeOrError)
                        .OnSuccess(() => new Registration(regTypeOrError.Value, examOption, row.SpecialArrangements))
                        .Map(x => x);

                registrationsResults.Add(registrationsError);
            }

            var cg = row.CandidateGuardian;
            var candidateGuardianResult = CandidateGuardian.CreateOptional(cg.OtherNames, cg.FamilyName, cg.Telephone, cg.Relationship);

            var childCollectionResult = Result.Ok(ChildCollection.Empty);
            if (candidateGuardianResult.IsSuccess)
                childCollectionResult = ChildCollection.Create(row.Candidate.ChildCollectionOption, candidateGuardianResult.Value);

            var candidateRegistrationResult = Result.Ok();
            Maybe<CandidateExamsRegistrations> candidateRegistration = null;
            if (candidateResult.IsSuccess) // .On
            {
                if (identifierResult.IsSuccess)
                {
                    candidateRegistration = allRegistrations.Find(candidateResult.Value, identifierResult.Value);

                    if (candidateRegistration.HasValue)
                    {
                        var result = CandidateExamsRegistrations.CreateNew(session, candidateResult.Value);
                        if (result.IsSuccess)
                        {
                            candidateRegistration = result.Value;
                            allRegistrations.Add(candidateRegistration.Value);
                        }

                        candidateRegistrationResult = Result.Combine(candidateRegistrationResult, result);
                    }
                }
            }

            var combinedRegistrationsResults = Result.Combine(registrationsResults.ToArray());

            return Result.Combine(candidateResult, identifierResult, combinedRegistrationsResults,
                candidateGuardianResult, childCollectionResult, candidateRegistrationResult)
                .OnSuccess(() =>
                    candidateRegistration.Value.AddNewRegistrationGroup(
                        registrationsResults.Select(x => x.Value),
                        preparationCentreInfo,
                        identifierResult.Value,
                        childCollectionResult.Value
                    ));
        }
    }
}
